// LiteOverlay.exe - Main Desktop Application
// Launches Microsoft Edge in --app mode (proper Chromium rendering)
// with a built-in lightweight HTTP server for local web files.

using System;
using System.Diagnostics;
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
        private static int serverPort = 18990;
        private static Process browserProcess;

        [STAThread]
        static void Main()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory;

            // Start internal HTTP Server
            if (!StartLocalServer())
            {
                MessageBox.Show("LiteOverlay HTTP Server start nahi ho saka.\nPort " + serverPort + " shayad busy hai.",
                    "LiteOverlay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string url = "http://localhost:" + serverPort + "/";

            // Find Edge or Chrome executable path
            string browserPath = FindBrowser();

            if (browserPath != null)
            {
                // Launch browser in --app mode (clean window, no tabs, no URL bar)
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = browserPath;
                psi.Arguments = "--app=" + url + " --window-size=1150,750 --disable-extensions --new-window";
                psi.UseShellExecute = false;

                try
                {
                    browserProcess = Process.Start(psi);
                    browserProcess.WaitForExit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Browser launch failed: " + ex.Message, "LiteOverlay", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                // Fallback: Open in default browser
                try
                {
                    Process.Start(url);
                    // Keep server alive for 1 hour, user closes manually
                    Thread.Sleep(3600000);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No browser found: " + ex.Message, "LiteOverlay", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Cleanup: stop HTTP server
            StopServer();
        }

        private static string FindBrowser()
        {
            // Check common Edge paths
            string[] edgePaths = new string[] {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
            };

            foreach (string path in edgePaths)
            {
                if (File.Exists(path)) return path;
            }

            // Check common Chrome paths
            string[] chromePaths = new string[] {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe")
            };

            foreach (string path in chromePaths)
            {
                if (File.Exists(path)) return path;
            }

            return null;
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
                            HttpListenerContext context = listener.GetContext();
                            ThreadPool.QueueUserWorkItem(delegate { ProcessRequest(context); });
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

                // Security: prevent directory traversal
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
}
