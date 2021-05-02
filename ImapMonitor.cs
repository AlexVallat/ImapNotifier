using MailKit;
using MailKit.Net.Imap;
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

		private readonly NotifyIcon _notifyIcon;

		private CancellationTokenSource? _cancellation;
		private CancellationTokenSource? _idleDone;
		private bool _showConfiguration;
		private bool _recalculateRecents;

		public ImapMonitor(NotifyIcon notifyIcon)
		{
			_notifyIcon = notifyIcon;
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

			while (true)
			{
				_cancellation = new CancellationTokenSource();

				try
				{
					await ReconnectAsync(_cancellation.Token);

					var inbox = _imapClient.Inbox;
					inbox.RecentChanged += OnInboxRecentChanged;
					inbox.MessageExpunged += OnMessageExpunged;
					// Set initial count on icon
					OnInboxRecentChanged(inbox, EventArgs.Empty);

					while(true)
					{
						// Spec says 29 minutes, but gmail does 10 minutes.
						_idleDone = new CancellationTokenSource(TimeSpan.FromMinutes(9));
						try
						{
							await _imapClient.IdleAsync(_idleDone.Token, _cancellation.Token);
						}
						catch(Exception ex) when (ex is ImapProtocolException || ex is IOException)
						{
							await ReconnectAsync(_cancellation.Token);
						}
						finally
						{
							_idleDone.Dispose();
							_idleDone = null;
						}

						if (_recalculateRecents)
						{
							_recalculateRecents = false;
							await inbox.StatusAsync(StatusItems.Recent);
						}
					}
				}
				catch (OperationCanceledException)
				{
					await _imapClient.DisconnectAsync(true);
					_cancellation.Dispose();
					_cancellation = null;
					if (_showConfiguration)
					{
						if (_notifyIcon.Invoke(ShowConfigurationDialog) == DialogResult.Abort)
						{
							Application.Exit();
							return;
						}
						// Otherwise, just re-connect without reloading config
					}
				}
				catch (Exception ex)
				{
					_notifyIcon.ShowError(ex.Message);
				}
			}
		}

		private void OnInboxRecentChanged(object? sender, EventArgs e)
		{
			_notifyIcon.Count = ((sender as IMailFolder)?.Recent) ?? 0;
		}
		private void OnMessageExpunged(object? sender, MessageEventArgs e)
		{
			if (_notifyIcon.Count > 0)
			{
				// If we are displaying an icon, schedule a reconnect so that the recent count gets updated
				_recalculateRecents = true;
				_idleDone?.Cancel();
			}
		}

		private async Task ReconnectAsync(CancellationToken cancellation)
		{
			if (!_imapClient.IsConnected)
			{
				await _imapClient.ConnectAsync(Settings.Instance.Server, Settings.Instance.Port ?? 143, Settings.Instance.UseSsl ?? false, cancellation);
			}

			if (!_imapClient.IsAuthenticated)
			{
				await _imapClient.AuthenticateAsync(Settings.Instance.Username, Settings.Instance.Password, cancellation);

				await _imapClient.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellation);
			}
		}

		private DialogResult ShowConfigurationDialog()
		{
			using var configuration = new Configuration();
			return configuration.ShowDialog();
		}

		public void ShowConfiguration()
		{
			if (_cancellation != null)
			{
				_notifyIcon.ShowIcon();
				_showConfiguration = true;
				_idleDone?.Cancel();
				_cancellation?.Cancel();
			}
		}
	}
}
