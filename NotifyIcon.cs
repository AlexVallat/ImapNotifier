using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ImapNotifier
{
	class NotifyIcon: ApplicationContext
	{
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool DestroyIcon(IntPtr hIcon);

		private readonly Font _font;
		private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
		private readonly WindowsFormsSynchronizationContext _synchronizationContext;
		private string? _errorMessage;

		public NotifyIcon()
		{
			_font = new Font(FontFamily.GenericSansSerif, SystemInformation.SmallIconSize.Height / 3);
			_notifyIcon = new System.Windows.Forms.NotifyIcon
			{
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
				_font.Dispose();
				ReplaceIcon(null);
				_notifyIcon.Dispose();
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
			Count = -1;
		}

		private void UpdateVisibility()
		{
			var iconSize = SystemInformation.SmallIconSize;
			var isLightTheme = (Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0) as int?) == 1;

			using var icon = LoadIconImage(isLightTheme, Math.Max(iconSize.Height, iconSize.Height));
			using var bitmap = new Bitmap(iconSize.Width, iconSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (var graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(icon, 0, 0, iconSize.Width, iconSize.Height);
				if (_count > 0)
				{
					graphics.DrawString(_count.ToString(), _font, isLightTheme ? Brushes.Black : Brushes.White, new RectangleF(0, icon.Height / 8, icon.Width, icon.Height),
						new StringFormat { Alignment = StringAlignment.Center });
				}
			}

			ReplaceIcon(bitmap.GetHicon());

			_notifyIcon.Text = _count > 0 ? $"{_count} messages" : Application.ProductName;
			_notifyIcon.Visible = _count != 0;
		}

		private static Bitmap LoadIconImage(bool isLightTheme, int size)
		{
			var bestSize = size switch
			{
				<= 16 => "16",
				<= 24 => "24",
				    _ => "32",
			};
			var colour = isLightTheme ? "Black" : "White";
			var iconFilename =  $"NotificationIcon.{colour}{bestSize}.png";
			using var iconStream = typeof(NotifyIcon).Assembly.GetManifestResourceStream(typeof(NotifyIcon), iconFilename)!;
			return new Bitmap(iconStream);
		}

		private void ReplaceIcon(IntPtr? newIcon)
		{
			var oldHandle = _notifyIcon.Icon?.Handle;

			_notifyIcon.Icon = newIcon.HasValue ? Icon.FromHandle(newIcon.Value) : null;
			if (oldHandle.HasValue)
			{
				DestroyIcon(oldHandle.Value);
			}
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
