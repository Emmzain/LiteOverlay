using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LiteOverlay
{
    static class Program
    {
        private static HttpListener listener;
        private static string appDir;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            appDir = AppDomain.CurrentDomain.BaseDirectory;

            // Start internal low-overhead HTTP Server on port 8080 if not already running
            StartLocalServer();

            Application.Run(new OverlayForm());
        }

        private static void StartLocalServer()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:8080/");
                listener.Start();

                ThreadPool.QueueUserWorkItem((o) =>
                {
                    while (listener.IsListening)
                    {
                        try
                        {
                            var context = listener.GetContext();
                            ProcessRequest(context);
                        }
                        catch { }
                    }
                });
            }
            catch (Exception ex)
            {
                // Server might already be running on 8080
                Console.WriteLine("Server note: " + ex.Message);
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string filename = context.Request.Url.AbsolutePath.TrimStart('/');
                if (string.IsNullOrEmpty(filename))
                {
                    filename = "index.html";
                }

                string filePath = Path.Combine(appDir, filename);

                if (File.Exists(filePath))
                {
                    byte[] b = File.ReadAllBytes(filePath);
                    string ext = Path.GetExtension(filePath).ToLower();

                    if (ext == ".html") context.Response.ContentType = "text/html";
                    else if (ext == ".css") context.Response.ContentType = "text/css";
                    else if (ext == ".js") context.Response.ContentType = "application/javascript";
                    else if (ext == ".json") context.Response.ContentType = "application/json";

                    context.Response.ContentLength64 = b.Length;
                    context.Response.OutputStream.Write(b, 0, b.Length);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch { }
            finally
            {
                context.Response.OutputStream.Close();
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
            this.TopMost = true; // Always On Top Over Games & Windows
            this.Icon = SystemIcons.Application;

            browser = new WebBrowser();
            browser.Dock = DockStyle.Fill;
            browser.ScriptErrorsSuppressed = true;
            browser.Url = new Uri("http://localhost:8080/");

            this.Controls.Add(browser);
        }
    }
}
