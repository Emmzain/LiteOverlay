// LiteOverlay.exe - Self-Contained Desktop Application
// All web files (HTML, CSS, JS) are EMBEDDED inside this exe.
// On first run, extracts files to its own directory, then launches Edge --app mode.
// Single download = complete working app with beautiful dark UI.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
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

        private static readonly string[] WEB_FILES = new string[] {
            "index.html", "styles.css", "app.js", "tauri_bridge.js"
        };

        [STAThread]
        static void Main()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory;

            // Step 1: Extract embedded web files if they don't exist
            ExtractEmbeddedFiles();

            // Step 2: Start internal HTTP Server
            if (!StartLocalServer())
            {
                serverPort = 19880;
                if (!StartLocalServer())
                {
                    serverPort = 20100;
                    if (!StartLocalServer())
                    {
                        MessageBox.Show(
                            "LiteOverlay HTTP Server start nahi ho saka.\nPorts busy hain.",
                            "LiteOverlay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            string url = "http://localhost:" + serverPort + "/";

            // Step 3: Find Edge or Chrome and launch in --app mode
            string browserPath = FindBrowser();

            if (browserPath != null)
            {
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
                catch
                {
                    // Fallback: open in default browser
                    LaunchDefaultBrowser(url);
                }
            }
            else
            {
                LaunchDefaultBrowser(url);
            }

            StopServer();
        }

        // ═══════════════════════════════════════════
        //  Extract embedded resource files
        // ═══════════════════════════════════════════
        private static void ExtractEmbeddedFiles()
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            foreach (string file in WEB_FILES)
            {
                string destPath = Path.Combine(appDir, file);

                // Always overwrite to keep files in sync with exe version
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

        // ═══════════════════════════════════════════
        //  Browser Detection
        // ═══════════════════════════════════════════
        private static string FindBrowser()
        {
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

        private static void LaunchDefaultBrowser(string url)
        {
            try
            {
                ProcessStartInfo defPsi = new ProcessStartInfo();
                defPsi.FileName = url;
                defPsi.UseShellExecute = true;
                Process.Start(defPsi);

                MessageBox.Show(
                    "LiteOverlay browser mein open ho gaya hai.\nYe message band karne se app band ho jayegi.\n\nOK press karein jab done ho.",
                    "LiteOverlay Running",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Browser nahi mila: " + ex.Message,
                    "LiteOverlay", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ═══════════════════════════════════════════
        //  HTTP Server
        // ═══════════════════════════════════════════
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
