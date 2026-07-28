// LiteOverlay.exe - Native Windows Desktop Application
// Native WinForms WebBrowser with IE11/Edge Emulation Mode (FEATURE_BROWSER_EMULATION = 11001)
// Embedded HTTP Server & Always-On-Top (TopMost = true) for Games.

using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace LiteOverlay
{
    static class Program
    {
        private static HttpListener listener;
        private static string appDir;
        private static int serverPort = 18990;

        private static readonly string[] WEB_FILES = new string[] {
            "index.html", "styles.css", "app.js", "tauri_bridge.js"
        };

        [STAThread]
        static void Main()
        {
            // Enable IE11/Edge High-Performance Rendering Mode in Registry
            EnableEdgeEmulation();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            appDir = AppDomain.CurrentDomain.BaseDirectory;

            // Extract embedded web assets
            ExtractEmbeddedFiles();

            // Start internal local HTTP server
            if (!StartLocalServer())
            {
                serverPort = 19880;
                if (!StartLocalServer())
                {
                    serverPort = 20100;
                    StartLocalServer();
                }
            }

            // Launch Native Windows Form Window
            Application.Run(new OverlayForm());

            StopServer();
        }

        private static void EnableEdgeEmulation()
        {
            try
            {
                string appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    if (key != null)
                    {
                        // 11001 = IE11 Standards Mode (full CSS variables & modern layout)
                        key.SetValue(appName, 11001, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        private static void ExtractEmbeddedFiles()
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            foreach (string file in WEB_FILES)
            {
                string destPath = Path.Combine(appDir, file);

                Stream stream = asm.GetManifestResourceStream(file);
                if (stream != null)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fs.Write(buffer, 0, bytesRead);
                            }
                        }
                        stream.Close();
                    }
                    catch { }
                }
            }
        }

        private static bool StartLocalServer()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:" + serverPort + "/");
                listener.Start();

                ThreadPool.QueueUserWorkItem(delegate
                {
                    while (listener != null && listener.IsListening)
                    {
                        try
                        {
                            HttpListenerContext ctx = listener.GetContext();
                            ThreadPool.QueueUserWorkItem(delegate { ProcessRequest(ctx); });
                        }
                        catch { }
                    }
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void StopServer()
        {
            try
            {
                if (listener != null)
                {
                    listener.Stop();
                    listener.Close();
                    listener = null;
                }
            }
            catch { }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string filename = context.Request.Url.AbsolutePath.TrimStart('/');
                if (string.IsNullOrEmpty(filename)) filename = "index.html";

                filename = filename.Replace("..", "");
                string filePath = Path.Combine(appDir, filename);

                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    string ext = Path.GetExtension(filePath).ToLower();

                    if (ext == ".html") context.Response.ContentType = "text/html; charset=utf-8";
                    else if (ext == ".css") context.Response.ContentType = "text/css; charset=utf-8";
                    else if (ext == ".js") context.Response.ContentType = "application/javascript; charset=utf-8";
                    else if (ext == ".json") context.Response.ContentType = "application/json";
                    else if (ext == ".png") context.Response.ContentType = "image/png";
                    else if (ext == ".ico") context.Response.ContentType = "image/x-icon";
                    else if (ext == ".svg") context.Response.ContentType = "image/svg+xml";
                    else context.Response.ContentType = "application/octet-stream";

                    context.Response.ContentLength64 = data.Length;
                    context.Response.StatusCode = 200;
                    context.Response.OutputStream.Write(data, 0, data.Length);
                }
                else
                {
                    byte[] msg = Encoding.UTF8.GetBytes("404 Not Found");
                    context.Response.StatusCode = 404;
                    context.Response.ContentLength64 = msg.Length;
                    context.Response.OutputStream.Write(msg, 0, msg.Length);
                }
            }
            catch { }
            finally
            {
                try { context.Response.OutputStream.Close(); } catch { }
            }
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
            browser.Url = new Uri("http://localhost:18990/");

            this.Controls.Add(browser);
        }
    }
}
