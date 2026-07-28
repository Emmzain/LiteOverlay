// LiteOverlay.exe - Main Desktop Application
// Launches Microsoft Edge in --app mode (Chromium rendering, same as browser)
// with a built-in lightweight HTTP server for local web files.
// The app window looks EXACTLY like the browser version with full CSS.

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

        [STAThread]
        static void Main()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory;

            // Start internal HTTP Server
            if (!StartLocalServer())
            {
                // Try alternate port
                serverPort = 19880;
                if (!StartLocalServer())
                {
                    MessageBox.Show(
                        "LiteOverlay HTTP Server start nahi ho saka.\nDono ports (18990, 19880) busy hain.",
                        "LiteOverlay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string url = "http://localhost:" + serverPort + "/";

            // Find Edge or Chrome
            string browserPath = FindBrowser();

            if (browserPath != null)
            {
                // Create a dedicated user-data-dir so Edge opens a SEPARATE clean app window
                // (not merged into existing browser tabs)
                string userDataDir = Path.Combine(appDir, "LiteOverlay_AppData");

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = browserPath;
                psi.Arguments = string.Format(
                    "--app={0} --window-size=1150,750 --disable-extensions --user-data-dir=\"{1}\"",
                    url, userDataDir);
                psi.UseShellExecute = false;

                try
                {
                    Process proc = Process.Start(psi);
                    if (proc != null)
                    {
                        proc.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    // If --app mode fails, try simple launch
                    try
                    {
                        ProcessStartInfo fallbackPsi = new ProcessStartInfo();
                        fallbackPsi.FileName = browserPath;
                        fallbackPsi.Arguments = url;
                        fallbackPsi.UseShellExecute = false;
                        Process fallbackProc = Process.Start(fallbackPsi);
                        if (fallbackProc != null) fallbackProc.WaitForExit();
                    }
                    catch
                    {
                        MessageBox.Show("Browser launch failed: " + ex.Message,
                            "LiteOverlay", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            else
            {
                // No Edge/Chrome found - open in whatever default browser
                try
                {
                    ProcessStartInfo defPsi = new ProcessStartInfo();
                    defPsi.FileName = url;
                    defPsi.UseShellExecute = true;
                    Process.Start(defPsi);

                    MessageBox.Show(
                        "LiteOverlay browser mein open ho gaya hai.\nYe window band mat karein - server chal raha hai.\n\nBand karne ke liye OK press karein.",
                        "LiteOverlay Running",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No browser found: " + ex.Message,
                        "LiteOverlay", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            StopServer();
        }

        private static string FindBrowser()
        {
            // Edge paths (Windows 10/11 pre-installed)
            string[] edgePaths = new string[] {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Microsoft", "Edge", "Application", "msedge.exe")
            };

            foreach (string p in edgePaths)
            {
                if (File.Exists(p)) return p;
            }

            // Chrome paths
            string[] chromePaths = new string[] {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Google", "Chrome", "Application", "chrome.exe")
            };

            foreach (string p in chromePaths)
            {
                if (File.Exists(p)) return p;
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
}
