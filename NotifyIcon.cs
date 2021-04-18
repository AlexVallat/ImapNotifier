using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ImapNotifier
{
	class NotifyIcon: IDisposable
	{
		private readonly ApplicationContext _applicationContext = new();
		private readonly Thread _thread;

		private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
		private readonly Icon _icon;
		private readonly string? _errorMessage;

		private bool _disposed;

		public NotifyIcon(int count) : this()
		{
			SetCount(count);
		}
		private NotifyIcon(string? errorMessage) : this()
		{
			_errorMessage = errorMessage;
		}

		private NotifyIcon()
		{
			var isLightTheme = (Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0) as int?) == 1;
			_icon = new Icon(typeof(NotifyIcon).Assembly.GetManifestResourceStream(typeof(NotifyIcon), isLightTheme ? "NotificationBlack.ico" : "Notification.ico")!, SystemInformation.SmallIconSize);
			_notifyIcon = new System.Windows.Forms.NotifyIcon
			{
				Icon = _icon,
				Text = Application.ProductName,
			};
			_notifyIcon.MouseClick += OnClick;
			_notifyIcon.BalloonTipClicked += OnBallonTipClicked;

			_thread = new Thread(Start)
			{
				Name = "NotifyIcon Message Loop",
			};
			_thread.SetApartmentState(ApartmentState.STA);
			_thread.Start();
		}

		public void SetCount(int count)
		{
			_notifyIcon.Text = $"{count} messages";
		}

		private void Start()
		{
			if (!_disposed)
			{
				try
				{
					_notifyIcon.Visible = true;
					if (_errorMessage != null)
					{
						ShowErrorBallonTip();
					}
					Application.Run(_applicationContext);
				}
				finally
				{
					_notifyIcon.Dispose();
					_icon.Dispose();
				}
			}
		}

		private void ShowErrorBallonTip()
		{
			_notifyIcon.ShowBalloonTip(0, "Error:", _errorMessage, ToolTipIcon.Error);
		}
		private void OnBallonTipClicked(object? sender, EventArgs e)
		{
			Dispose();
		}

		private static NotifyIcon? _errorIcon;
		private static readonly object _errorIconLock = new();
		public static void ShowError(string message)
		{
			// Only have one error showing at a time
			lock (_errorIconLock)
			{
				_errorIcon?.Dispose();
				_errorIcon = new NotifyIcon(message);
				// Error notify icons are self-disposing
			}
		}

		private void OnClick(object? sender, MouseEventArgs e)
		{
			if (_errorMessage != null)
			{
				ShowErrorBallonTip();
			}
			else if (!string.IsNullOrEmpty(Settings.Instance.OpenEmail))
			{
				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = Settings.Instance.OpenEmail,
						UseShellExecute = true
					});
				}
				catch (Exception ex)
				{
					ShowError(Settings.Instance.OpenEmail + "\n\n" + ex.Message);
				}
			}
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				_applicationContext.ExitThread();
				_applicationContext.Dispose();
			}
		}
	}
}
