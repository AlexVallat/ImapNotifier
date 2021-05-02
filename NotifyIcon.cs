using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ImapNotifier
{
	class NotifyIcon: ApplicationContext
	{
		private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
		private readonly Icon _icon;
		private readonly WindowsFormsSynchronizationContext _synchronizationContext;

		private string? _errorMessage;

		public NotifyIcon()
		{
			var isLightTheme = (Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0) as int?) == 1;
			using (var iconStream = typeof(NotifyIcon).Assembly.GetManifestResourceStream(typeof(NotifyIcon), isLightTheme ? "NotificationBlack.ico" : "Notification.ico")!)
			{
				_icon = new Icon(iconStream, SystemInformation.SmallIconSize);
			}
			_notifyIcon = new System.Windows.Forms.NotifyIcon
			{
				Icon = _icon,
				Text = Application.ProductName,
				Visible = true
			};
			_notifyIcon.MouseClick += OnClick;
			_notifyIcon.BalloonTipClicked += OnBallonTipClicked;

			ThreadExit += OnThreadExit;

			_synchronizationContext = SynchronizationContext.Current as WindowsFormsSynchronizationContext ?? CreateSyncContext();
			WindowsFormsSynchronizationContext CreateSyncContext()
			{
				var syncContext = new WindowsFormsSynchronizationContext();
				SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
				return syncContext;
			}
		}

		private void OnThreadExit(object? sender, EventArgs e)
		{
			_notifyIcon.Visible = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_notifyIcon.Dispose();
				_icon.Dispose();
			}
			base.Dispose(disposing);
		}

		private int _count;
		public int Count
		{
			get => _count;
			set
			{
				_count = value;
				Invoke(UpdateVisibility);
			}
		}

		public void Invoke(Action action)
		{
			_synchronizationContext.Post(_ =>
				action()
			, null);
		}

		public T? Invoke<T>(Func<T> func)
		{
			T? result = default;
			_synchronizationContext.Send(_ =>
				result = func()
			, null);

			return result;
		}

		public void ShowIcon()
		{
			Invoke(() => { _notifyIcon.Visible = true; });
		}

		private void UpdateVisibility()
		{
			_notifyIcon.Text = _count > 0 ? $"{_count} messages" : Application.ProductName;
			_notifyIcon.Visible = _count > 0;
		}

		public void ShowError(string message)
		{
			_errorMessage = message;
			Invoke(delegate
			{
				_notifyIcon.Text = Application.ProductName + " Error";
				_notifyIcon.Visible = true;
				_notifyIcon.ShowBalloonTip(0, "Error:", message, ToolTipIcon.Error);
			});
		}

		private void OnBallonTipClicked(object? sender, EventArgs e)
		{
			_errorMessage = null;
			Invoke(UpdateVisibility);
		}

		private void OnClick(object? sender, MouseEventArgs e)
		{
			if (_errorMessage != null)
			{
				ShowError(_errorMessage);
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
	}
}
