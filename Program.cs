using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailKit.Net.Imap;
using Microsoft.Win32;
using SingleInstanceHelper;

namespace ImapNotifier
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
#if !DEBUG2
            try
#endif
            {
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var imapMonitor = new ImapMonitor();

                if (await ApplicationActivator.LaunchOrReturnAsync(_ => imapMonitor.ShowConfiguration()))
                {
                    await imapMonitor.Run();
                }

                Application.Exit();
                Application.DoEvents(); // Ensure the quit message is processed
            }
#if !DEBUG2
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
