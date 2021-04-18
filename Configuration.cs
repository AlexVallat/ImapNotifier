using MailKit.Net.Imap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImapNotifier
{
	public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
            _server.Text = Settings.Instance.Server;
			_port.Value = Settings.Instance.Port ?? 143;
			_ssl.Checked= Settings.Instance.UseSsl ?? false;
			_username.Text= Settings.Instance.Username;
			_password.Text= Settings.Instance.Password;
			_openEmail.Text = Settings.Instance.OpenEmail;
			_startup.Checked = Settings.Instance.StartWithWindows;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			Activate();
		}

		private async void _ok_Click(object sender, EventArgs e)
		{
            if (await TestConnection())
			{
				Settings.Instance.Server = _server.Text;
				Settings.Instance.Port = (int)_port.Value;
				Settings.Instance.UseSsl = _ssl.Checked;
				Settings.Instance.Username = _username.Text;
				Settings.Instance.Password = _password.Text;
				Settings.Instance.OpenEmail = _openEmail.Text;
				Settings.Instance.StartWithWindows = _startup.Checked;
				Settings.Instance.Save();

                DialogResult = DialogResult.OK;
			}
		}

		private void SetAllControlsDisabled(Control control, bool enabled)
		{
			foreach (var child in control.Controls.OfType<Control>())
			{
				child.Enabled = enabled;
				SetAllControlsDisabled(child, enabled);
			}
		}

        private async Task<bool> TestConnection()
		{
			UseWaitCursor = true;
			SetAllControlsDisabled(this, false);

			var cancellation = new CancellationTokenSource();
			void Cancel(object? sender, EventArgs e)
			{
				cancellation.Cancel();
			}

			_cancel.Enabled = true; // Allow cancellation
			_cancel.DialogResult = DialogResult.None;
			_cancel.Click += Cancel;
			try
			{
				var imapClient = new ImapClient();
				await imapClient.ConnectAsync(_server.Text, (int)_port.Value, _ssl.Checked, cancellation.Token);
				await imapClient.AuthenticateAsync(_username.Text, _password.Text, cancellation.Token);
				if (imapClient.Capabilities.HasFlag(ImapCapabilities.Idle))
				{
					return true;
				}

				MessageBox.Show(this, "IMAP server does not support IDLE", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (TaskCanceledException) { }
			catch (Exception ex)
			{
				MessageBox.Show(this, "Unable to connect to IMAP server:\n\n" + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				UseWaitCursor = false;
				SetAllControlsDisabled(this, true);

				_cancel.DialogResult = DialogResult.Cancel;
				_cancel.Click -= Cancel;
			}
			return false;
		}

		private void _ssl_Click(object sender, EventArgs e)
		{
			_port.Value = _ssl.Checked ? 993 : 143;
		}
	}
}
