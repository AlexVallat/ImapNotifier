
namespace ImapNotifier
{
    partial class Configuration
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.Windows.Forms.Label _serverLabel;
			System.Windows.Forms.Label _usernameLabel;
			System.Windows.Forms.Label _passwordLabel;
			System.Windows.Forms.GroupBox _imapSettings;
			System.Windows.Forms.Label openEmailLabel;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Configuration));
			this._ssl = new System.Windows.Forms.CheckBox();
			this._server = new System.Windows.Forms.TextBox();
			this._password = new System.Windows.Forms.TextBox();
			this._port = new System.Windows.Forms.NumericUpDown();
			this._username = new System.Windows.Forms.TextBox();
			this._ok = new System.Windows.Forms.Button();
			this._exit = new System.Windows.Forms.Button();
			this._startup = new System.Windows.Forms.CheckBox();
			this._cancel = new System.Windows.Forms.Button();
			this._openEmail = new System.Windows.Forms.TextBox();
			_serverLabel = new System.Windows.Forms.Label();
			_usernameLabel = new System.Windows.Forms.Label();
			_passwordLabel = new System.Windows.Forms.Label();
			_imapSettings = new System.Windows.Forms.GroupBox();
			openEmailLabel = new System.Windows.Forms.Label();
			_imapSettings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._port)).BeginInit();
			this.SuspendLayout();
			// 
			// _serverLabel
			// 
			_serverLabel.AutoSize = true;
			_serverLabel.Location = new System.Drawing.Point(6, 19);
			_serverLabel.Name = "_serverLabel";
			_serverLabel.Size = new System.Drawing.Size(42, 15);
			_serverLabel.TabIndex = 1;
			_serverLabel.Text = "&Server:";
			// 
			// _usernameLabel
			// 
			_usernameLabel.AutoSize = true;
			_usernameLabel.Location = new System.Drawing.Point(5, 71);
			_usernameLabel.Name = "_usernameLabel";
			_usernameLabel.Size = new System.Drawing.Size(60, 15);
			_usernameLabel.TabIndex = 5;
			_usernameLabel.Text = "&Username";
			// 
			// _passwordLabel
			// 
			_passwordLabel.AutoSize = true;
			_passwordLabel.Location = new System.Drawing.Point(5, 120);
			_passwordLabel.Name = "_passwordLabel";
			_passwordLabel.Size = new System.Drawing.Size(57, 15);
			_passwordLabel.TabIndex = 7;
			_passwordLabel.Text = "&Password";
			// 
			// _imapSettings
			// 
			_imapSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			_imapSettings.Controls.Add(this._ssl);
			_imapSettings.Controls.Add(_serverLabel);
			_imapSettings.Controls.Add(this._server);
			_imapSettings.Controls.Add(this._password);
			_imapSettings.Controls.Add(this._port);
			_imapSettings.Controls.Add(_passwordLabel);
			_imapSettings.Controls.Add(_usernameLabel);
			_imapSettings.Controls.Add(this._username);
			_imapSettings.Location = new System.Drawing.Point(12, 12);
			_imapSettings.Name = "_imapSettings";
			_imapSettings.Size = new System.Drawing.Size(386, 173);
			_imapSettings.TabIndex = 0;
			_imapSettings.TabStop = false;
			_imapSettings.Text = "Imap Settings";
			// 
			// _ssl
			// 
			this._ssl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._ssl.AutoSize = true;
			this._ssl.Location = new System.Drawing.Point(313, 67);
			this._ssl.Name = "_ssl";
			this._ssl.Size = new System.Drawing.Size(67, 19);
			this._ssl.TabIndex = 4;
			this._ssl.Text = "SS&L/TLS";
			this._ssl.UseVisualStyleBackColor = true;
			this._ssl.Click += new System.EventHandler(this._ssl_Click);
			// 
			// _server
			// 
			this._server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._server.Location = new System.Drawing.Point(6, 37);
			this._server.Name = "_server";
			this._server.Size = new System.Drawing.Size(308, 23);
			this._server.TabIndex = 2;
			// 
			// _password
			// 
			this._password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._password.Location = new System.Drawing.Point(5, 139);
			this._password.Name = "_password";
			this._password.Size = new System.Drawing.Size(375, 23);
			this._password.TabIndex = 8;
			this._password.UseSystemPasswordChar = true;
			// 
			// _port
			// 
			this._port.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._port.Location = new System.Drawing.Point(320, 37);
			this._port.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
			this._port.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this._port.Name = "_port";
			this._port.Size = new System.Drawing.Size(60, 23);
			this._port.TabIndex = 3;
			this._port.Value = new decimal(new int[] {
            143,
            0,
            0,
            0});
			// 
			// _username
			// 
			this._username.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._username.Location = new System.Drawing.Point(5, 90);
			this._username.Name = "_username";
			this._username.Size = new System.Drawing.Size(375, 23);
			this._username.TabIndex = 6;
			// 
			// openEmailLabel
			// 
			openEmailLabel.AutoSize = true;
			openEmailLabel.Location = new System.Drawing.Point(12, 188);
			openEmailLabel.Name = "openEmailLabel";
			openEmailLabel.Size = new System.Drawing.Size(131, 15);
			openEmailLabel.TabIndex = 9;
			openEmailLabel.Text = "&Open Email Command:";
			// 
			// _ok
			// 
			this._ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._ok.Location = new System.Drawing.Point(242, 273);
			this._ok.Name = "_ok";
			this._ok.Size = new System.Drawing.Size(75, 23);
			this._ok.TabIndex = 12;
			this._ok.Text = "OK";
			this._ok.UseVisualStyleBackColor = true;
			this._ok.Click += new System.EventHandler(this._ok_Click);
			// 
			// _exit
			// 
			this._exit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._exit.DialogResult = System.Windows.Forms.DialogResult.Abort;
			this._exit.Location = new System.Drawing.Point(13, 273);
			this._exit.Name = "_exit";
			this._exit.Size = new System.Drawing.Size(75, 23);
			this._exit.TabIndex = 14;
			this._exit.Text = "E&xit";
			this._exit.UseVisualStyleBackColor = true;
			// 
			// _startup
			// 
			this._startup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._startup.AutoSize = true;
			this._startup.Location = new System.Drawing.Point(13, 241);
			this._startup.Name = "_startup";
			this._startup.Size = new System.Drawing.Size(128, 19);
			this._startup.TabIndex = 11;
			this._startup.Text = "S&tart with Windows";
			this._startup.UseVisualStyleBackColor = true;
			// 
			// _cancel
			// 
			this._cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this._cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancel.Location = new System.Drawing.Point(323, 273);
			this._cancel.Name = "_cancel";
			this._cancel.Size = new System.Drawing.Size(75, 23);
			this._cancel.TabIndex = 13;
			this._cancel.Text = "Cancel";
			this._cancel.UseVisualStyleBackColor = true;
			// 
			// _openEmail
			// 
			this._openEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._openEmail.Location = new System.Drawing.Point(12, 207);
			this._openEmail.Name = "_openEmail";
			this._openEmail.Size = new System.Drawing.Size(386, 23);
			this._openEmail.TabIndex = 10;
			// 
			// Configuration
			// 
			this.AcceptButton = this._ok;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancel;
			this.ClientSize = new System.Drawing.Size(410, 308);
			this.Controls.Add(this._cancel);
			this.Controls.Add(this._startup);
			this.Controls.Add(_imapSettings);
			this.Controls.Add(this._exit);
			this.Controls.Add(this._ok);
			this.Controls.Add(openEmailLabel);
			this.Controls.Add(this._openEmail);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Configuration";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Imap Notifier Configuration";
			_imapSettings.ResumeLayout(false);
			_imapSettings.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._port)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _ok;
        private System.Windows.Forms.TextBox _server;
		private System.Windows.Forms.Button _exit;
		private System.Windows.Forms.NumericUpDown _port;
		private System.Windows.Forms.TextBox _username;
		private System.Windows.Forms.TextBox _password;
		private System.Windows.Forms.CheckBox _ssl;
		private System.Windows.Forms.CheckBox _startup;
		private System.Windows.Forms.Button _cancel;
		private System.Windows.Forms.TextBox _openEmail;
	}
}

