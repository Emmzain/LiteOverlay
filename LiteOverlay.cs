// LiteOverlay.exe - Pure Standalone Native Windows Application
// Zero localhost servers, zero external browser processes, zero connection errors.
// Loads self-contained HTML/CSS/JS directly inside native C# window with TopMost overlay.

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace LiteOverlay
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Enable IE11 Standards Mode rendering in Registry
            EnableEdgeEmulation();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Launch Native C# Window
            Application.Run(new OverlayForm());
        }

        private static void EnableEdgeEmulation()
        {
            try
            {
                string appName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    if (key != null)
                    {
                        key.SetValue(appName, 11001, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }
    }

    public class OverlayForm : Form
    {
        private WebBrowser browser;

        public OverlayForm()
        {
            this.Text = "LiteOverlay System Monitor";
            this.Size = new Size(1150, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true; // Always On Top Over Games
            this.BackColor = Color.FromArgb(12, 14, 18);
            this.Icon = SystemIcons.Application;

            browser = new WebBrowser();
            browser.Dock = DockStyle.Fill;
            browser.ScriptErrorsSuppressed = true;
            browser.IsWebBrowserContextMenuEnabled = false;

            this.Controls.Add(browser);

            LoadNativeDocument();
        }

        private void LoadNativeDocument()
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();

                string html = ReadResourceStream(asm, "index.html");
                string css = ReadResourceStream(asm, "styles.css");
                string jsApp = ReadResourceStream(asm, "app.js");
                string jsTauri = ReadResourceStream(asm, "tauri_bridge.js");

                // Inject CSS and JS directly into HTML document
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html><head>");
                sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
                sb.AppendLine("<meta charset=\"UTF-8\">");
                sb.AppendLine("<title>LiteOverlay</title>");
                sb.AppendLine("<style>");
                sb.AppendLine(css);
                sb.AppendLine("</style>");
                sb.AppendLine("</head><body class=\"theme-dark\">");

                // Extract body content from index.html
                int bodyStart = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                if (bodyStart >= 0)
                {
                    int bodyContentStart = html.IndexOf('>', bodyStart) + 1;
                    int bodyEnd = html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                    if (bodyEnd > bodyContentStart)
                    {
                        sb.AppendLine(html.Substring(bodyContentStart, bodyEnd - bodyContentStart));
                    }
                    else
                    {
                        sb.AppendLine(html);
                    }
                }
                else
                {
                    sb.AppendLine(html);
                }

                sb.AppendLine("<script>");
                sb.AppendLine(jsTauri);
                sb.AppendLine(jsApp);
                sb.AppendLine("</script>");
                sb.AppendLine("</body></html>");

                browser.DocumentText = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Native Document Load Error: " + ex.Message, "LiteOverlay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ReadResourceStream(Assembly asm, string resourceName)
        {
            using (Stream stream = asm.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return string.Empty;
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
