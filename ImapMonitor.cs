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

		private NotifyIcon? _notifyIcon;

		private CancellationTokenSource? _cancellation;
		private CancellationTokenSource? _idleDone;
		private bool _showConfiguration;
		private bool _recalculateRecents;

		public async Task Run()
		{
			if (string.IsNullOrEmpty(Settings.Instance.Username) ||
					string.IsNullOrEmpty(Settings.Instance.Password) ||
					string.IsNullOrEmpty(Settings.Instance.Server) ||
					Settings.Instance.Port == null ||
					Settings.Instance.UseSsl == null)
			{
				if (ShowConfigurationDialog() != DialogResult.OK)
				{
					return;
				}
			}

			var firstRunIcon = new NotifyIcon(0);

			while (true)
			{
				_cancellation = new CancellationTokenSource();

				try
				{
					await ReconnectAsync(_cancellation.Token);

					if (firstRunIcon != null)
					{
						firstRunIcon.Dispose();
						firstRunIcon = null;
					}

					var inbox = _imapClient.Inbox;
					inbox.RecentChanged += OnInboxRecentChanged;
					inbox.MessageExpunged += OnMessageExpunged;
					// Show initial icon, if required
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
						if (ShowConfigurationDialog() == DialogResult.Abort)
						{
							// Exit
							return;
						}
						// Otherwise, just re-connect without reloading config
					}
				}
				catch (Exception ex)
				{
					NotifyIcon.ShowError(ex.Message);
				}
			}
		}

		private void OnInboxRecentChanged(object? sender, EventArgs e)
		{
			var recent = (sender as IMailFolder)?.Recent;
			if (recent > 0)
			{
				if (_notifyIcon == null)
				{
					_notifyIcon = new(recent.Value);
				}
				else
				{
					_notifyIcon.SetCount(recent.Value);
				}
			}
			else
			{
				_notifyIcon?.Dispose();
				_notifyIcon = null;
			}
		}
		private void OnMessageExpunged(object? sender, MessageEventArgs e)
		{
			if (_notifyIcon != null)
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

		private static DialogResult ShowConfigurationDialog()
		{
			using var configuration = new Configuration();
			return configuration.ShowDialog();
		}

		public void ShowConfiguration()
		{
			if (_cancellation != null)
			{
				_showConfiguration = true;
				_idleDone?.Cancel();
				_cancellation?.Cancel();
			}
		}
	}
}
