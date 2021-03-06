using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImapNotifier
{
	static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
#if !DEBUG
            try
#endif
            {
                using var instanceManager = SingleInstanceManager.SingleInstanceManager.CreateManager();
                if (instanceManager.RunApplication(args)) {
                    Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

#if LOG
                    using var logWriter = new StreamWriter(File.Open(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!, "ImapNotifier.log"), FileMode.Append | FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read));
#else
                    var logWriter = StreamWriter.Null;
#endif

                    using var notifyIcon = new NotifyIcon();
                    notifyIcon.ShowIcon();

                    var imapMonitor = new ImapMonitor(notifyIcon, logWriter);

                    instanceManager.SecondInstanceStarted += delegate
                    {
                        imapMonitor.ShowConfiguration();
                    };

                    Task.Run(imapMonitor.Run);

                    Application.Run(notifyIcon);
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                TaskDialog.ShowDialog(new TaskDialogPage
                {
                    Caption = Application.ProductName,
                    Heading = $"Fatal Error in {Application.ProductName}",
                    Text = $"{Application.ProductName} encountered an unhandled exception and has to close.",
                    Expander = new TaskDialogExpander
                    {
                        CollapsedButtonText = "Details",
                        Text = ex.ToString(),
                        Position = TaskDialogExpanderPosition.AfterText
                    },
                    Icon = TaskDialogIcon.Error,
                    Buttons = new TaskDialogButtonCollection
                    {
                        TaskDialogButton.Close
                    }
                });
            }
#endif
        }
    }
}
