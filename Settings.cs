using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace ImapNotifier
{
	internal class Settings
    {
        public string? Server { get; set; }
        public int? Port { get; set; }
        public bool? UseSsl { get; set; }
        public string? Username { get; set; }
        [JsonConverter(typeof(Encrypted))]
        public string? Password { get; set; }
        public string? OpenEmail { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Consistency")]
		public bool StartWithWindows
		{
            set
            {
				using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (key != null)
                {
                    if (value)
                    {
                        key.SetValue(Application.ProductName, "\"" + Application.ExecutablePath);
                    }
                    else
                    {
                        key.DeleteValue(Application.ProductName, false);
                    }
                }
			}
            get
            {
				using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
				return key?.GetValue(Application.ProductName) != null;
			}
        }

		#region Implementation
		private static readonly string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName, "settings.json");
        private static readonly Lazy<Settings> _instance = new(Load);

        public static Settings Instance => _instance.Value;

		private static Settings Load()
        {
            if (File.Exists(Path))
            {
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path)) ?? new Settings();
            }

            return new Settings();
        }

        public void Save()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            File.WriteAllText(Path, JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }

        private class Encrypted : JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    return Encoding.UTF8.GetString(ProtectedData.Unprotect(reader.GetBytesFromBase64(), null, DataProtectionScope.CurrentUser));
                }
                catch (CryptographicException)
                {
                    return string.Empty;
                }
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                writer.WriteBase64StringValue(ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser));
            }
        }

        #endregion
    }
}
