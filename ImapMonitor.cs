using MailKit;
using MailKit.Net.Imap;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImapNotifier
{
	class ImapMonitor
	{
		private readonly ImapClient _imapClient = new(
#if DEBUG
			new ProtocolLogger(new TraceLogStream())
#endif
			);

		private static readonly TimeSpan[] _backOffRetry = new[] {
			TimeSpan.FromSeconds(1),
			TimeSpan.FromSeconds(5),
			TimeSpan.FromSeconds(30),
			TimeSpan.FromMinutes(1),
			TimeSpan.FromMinutes(10),
			TimeSpan.FromMinutes(30),
			TimeSpan.FromHours(4),
			TimeSpan.FromDays(1),
		};

		private readonly NotifyIcon _notifyIcon;
        private readonly StreamWriter _logWriter;
        private CancellationTokenSource? _cancellation;
		private CancellationTokenSource? _idleDone;
		private CancellationTokenSource? _interruptExceptionBackOff;
		private TaskCompletionSource? _resume;
		private bool _showConfiguration;
		private bool _recalculateCount;

		public ImapMonitor(NotifyIcon notifyIcon, StreamWriter logWriter)
		{
			_notifyIcon = notifyIcon;
            _logWriter = logWriter;
            _notifyIcon.ErrorDismissed += (_, _) => _interruptExceptionBackOff?.Cancel();
			SystemEvents.SessionEnding += delegate { Application.Exit(); };
			SystemEvents.PowerModeChanged += OnPowerModeChanged;
		}

		[System.Diagnostics.Conditional("LOG")]
		private void Log(string message)
        {
			_logWriter.WriteLine($"{DateTime.Now:s}: {message}");
			_logWriter.Flush();
        }

		private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			switch (e.Mode)
			{
				case PowerModes.Suspend:
					Log("Suspending");
					if (_cancellation != null)
					{
						_resume = new TaskCompletionSource();

						Log("Cancelling due to suspension");
						_cancellation.Cancel();
					}
					break;
				case PowerModes.Resume:
					Log($"Resuming: {(_resume == null ? "no signal" : "signalling")}");
					_resume?.SetResult();
					break;
			}
		}

		public async Task Run()
		{
			if (string.IsNullOrEmpty(Settings.Instance.Username) ||
					string.IsNullOrEmpty(Settings.Instance.Password) ||
					string.IsNullOrEmpty(Settings.Instance.Server) ||
					Settings.Instance.Port == null ||
					Settings.Instance.UseSsl == null)
			{
				if (_notifyIcon.Invoke(ShowConfigurationDialog) != DialogResult.OK)
				{
					Application.Exit();
					return;
				}
			}

			var backOffRetryIndex = 0;
			IMailFolder? inbox = null;

			while (true)
			{
				_cancellation = new CancellationTokenSource();

				try
				{
					await ReconnectAsync(_cancellation.Token);

					if (!ReferenceEquals(inbox, _imapClient.Inbox))
					{
						Log("Connecting Inbox Event Handlers");
						if (inbox != null)
						{
							inbox.RecentChanged -= OnInboxCountChanged;
							inbox.CountChanged -= OnInboxCountChanged;
							inbox.MessageExpunged -= OnMessageExpunged;
						}

						inbox = _imapClient.Inbox;
						switch (Settings.Instance.CountType)
						{
							case CountType.Recent:
								inbox.RecentChanged += OnInboxCountChanged;
								break;
							case CountType.Exists:
								inbox.CountChanged += OnInboxCountChanged;
								break;
						}
						inbox.MessageExpunged += OnMessageExpunged;
					}

					// Set initial count on icon
					OnInboxCountChanged(inbox, EventArgs.Empty);

					while(true)
					{
						// Spec says 29 minutes, but gmail does 10 minutes.
						_idleDone = new CancellationTokenSource(TimeSpan.FromMinutes(9));
						try
						{
							Log("Idling");
							await _imapClient.IdleAsync(_idleDone.Token, _cancellation.Token);
						}
						catch(Exception ex) when (ex is ImapProtocolException || ex is IOException)
						{
							Log($"Reconnecting after exception on Idle: {ex.Message}");
							await ReconnectAsync(_cancellation.Token);
						}
						finally
						{
							_idleDone.Dispose();
							_idleDone = null;
						}

						// Reset retry timings
						backOffRetryIndex = 0;

						if (_recalculateCount)
						{
							Log("Forced recalculation");
							_recalculateCount = false;
							await inbox.StatusAsync(
								Settings.Instance.CountType switch
								{
									CountType.Recent => StatusItems.Recent,
									CountType.Exists => StatusItems.Count,
									_ => StatusItems.None
								});
						}
					}
				}
				catch (OperationCanceledException)
				{
					Log("Connection cancelled, disconnecting");

					await _imapClient.DisconnectAsync(true);
					_cancellation.Dispose();
					_cancellation = null;

					if (_resume != null)
					{
						Log("Awaiting resume");

						await _resume.Task;
						_resume = null;

						Log("Waiting for network");

						while (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
						{
							await Task.Delay(TimeSpan.FromSeconds(1));
						}
					}

					if (_showConfiguration)
					{
						Log("Showing Config UI");

						if (_notifyIcon.Invoke(ShowConfigurationDialog) == DialogResult.Abort)
						{
							Log("Exiting");

							Application.Exit();
							return;
						}
						// Otherwise, just re-connect without reloading config
					}
				}
				catch (Exception ex)
				{
					_notifyIcon.ShowError(ex.Message);
					var delay = _backOffRetry[Math.Max(backOffRetryIndex++, _backOffRetry.Length - 1)];
					_interruptExceptionBackOff = new CancellationTokenSource();

					Log($"Error: {ex.Message}, waiting {delay}");

					await Task.Delay(delay, _interruptExceptionBackOff.Token);

					Log("Retrying after error");

					_interruptExceptionBackOff.Dispose();
					_interruptExceptionBackOff = null;
				}
			}
		}

		private void OnInboxCountChanged(object? sender, EventArgs e)
		{
			if (sender is IMailFolder inbox)
			{
				Log($"Inbox count changed: {inbox.Recent}/{inbox.Count}");
				_notifyIcon.Count = Settings.Instance.CountType switch
				{
					CountType.Recent => inbox.Recent,
					CountType.Exists => inbox.Count,
					_ => 0
				};
			}
			else
			{
				_notifyIcon.Count = 0;
			}
		}
		private void OnMessageExpunged(object? sender, MessageEventArgs e)
		{
			Log("Message Expunged");
			if (_notifyIcon.Count > 0)
			{
				// If we are displaying an icon, schedule a reconnect so that the recent count gets updated
				_recalculateCount = true;
				_idleDone?.Cancel();
			}
		}

		private async Task ReconnectAsync(CancellationToken cancellation)
		{
			if (!_imapClient.IsConnected)
			{
				Log("Connecting...");
				await _imapClient.ConnectAsync(Settings.Instance.Server, Settings.Instance.Port ?? 143, Settings.Instance.UseSsl ?? false, cancellation);
			}

			if (!_imapClient.IsAuthenticated)
			{
				Log("Authenticating...");

				await _imapClient.AuthenticateAsync(Settings.Instance.Username, Settings.Instance.Password, cancellation);

				await _imapClient.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellation);
			}
			Log("Connected");
		}

		private DialogResult ShowConfigurationDialog()
		{
			using var configuration = new Configuration();
			return configuration.ShowDialog();
		}

		public void ShowConfiguration()
		{
			Log($"{(_cancellation == null ? "Unable to " : "")}Request Config UI");
			if (_cancellation != null)
			{
				_notifyIcon.ShowIcon();
				_showConfiguration = true;
				_idleDone?.Cancel();
				_cancellation?.Cancel();
				_interruptExceptionBackOff?.Cancel();
			}
		}
	}
}
