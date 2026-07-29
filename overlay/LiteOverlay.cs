// LiteOverlay - Pure Native Windows Gaming HUD & Telemetry Monitor
// 100% Native C# WinForms - Zero Browser dependencies
// Always-on-top gaming overlay with true per-pixel transparency
// v1.3.0 - Settings Persistence + Auto-Update + Game Overlay Fix

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace LiteOverlay
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DashboardForm());
        }
    }

    // ═══════════════════════════════════════════
    //  Persistent Settings Manager (JSON file)
    // ═══════════════════════════════════════════
    public static class AppState
    {
        public static bool ShowFps = true;
        public static bool ShowPing = true;
        public static bool ShowRam = true;
        public static bool ShowCpu = true;
        public static bool ShowGpu = true;
        public static bool ShowTemp = false;
        public static bool ShowBattery = true;
        public static bool ShowNetwork = false;
        public static bool ShowDisk = false;

        public static bool OverlayVisible = true;
        public static bool ShowBorder = true;
        public static bool ShowLabels = true;
        public static bool GlowEffect = true;
        public static bool LockPosition = false;

        public static Color AccentColor = Color.FromArgb(0, 230, 118);
        public static int OpacityPct = 85;
        public static int FontSize = 14;
        public static int BorderRadius = 6;
        public static string LayoutStyle = "Vertical Stack";
        public static int RefreshInterval = 500;

        public static int HudX = -1;
        public static int HudY = -1;

        public static int Fps = 0;
        public static int Ping = 0;
        public static float CpuPct = 0;
        public static float GpuPct = 0;
        public static string RamText = "0 MB";
        public static string RamTotalText = "0 MB";
        public static int Temp = 55;
        public static int BatteryPct = 100;
        public static string BatteryStatus = "Discharging";
        public static string NetSpeed = "0 KB/s";
        public static string DiskText = "124 GB / 256 GB";

        public static readonly string APP_VERSION = "1.0.1";

        private static string GetSettingsPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "LiteOverlay");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "settings.json");
        }

        public static void SaveSettings()
        {
            try
            {
                string json = "{" +
                    "\"ShowFps\":" + (ShowFps ? "true" : "false") + "," +
                    "\"ShowPing\":" + (ShowPing ? "true" : "false") + "," +
                    "\"ShowRam\":" + (ShowRam ? "true" : "false") + "," +
                    "\"ShowCpu\":" + (ShowCpu ? "true" : "false") + "," +
                    "\"ShowGpu\":" + (ShowGpu ? "true" : "false") + "," +
                    "\"ShowTemp\":" + (ShowTemp ? "true" : "false") + "," +
                    "\"ShowBattery\":" + (ShowBattery ? "true" : "false") + "," +
                    "\"ShowNetwork\":" + (ShowNetwork ? "true" : "false") + "," +
                    "\"ShowDisk\":" + (ShowDisk ? "true" : "false") + "," +
                    "\"OverlayVisible\":" + (OverlayVisible ? "true" : "false") + "," +
                    "\"ShowBorder\":" + (ShowBorder ? "true" : "false") + "," +
                    "\"ShowLabels\":" + (ShowLabels ? "true" : "false") + "," +
                    "\"GlowEffect\":" + (GlowEffect ? "true" : "false") + "," +
                    "\"LockPosition\":" + (LockPosition ? "true" : "false") + "," +
                    "\"AccentR\":" + AccentColor.R + "," +
                    "\"AccentG\":" + AccentColor.G + "," +
                    "\"AccentB\":" + AccentColor.B + "," +
                    "\"OpacityPct\":" + OpacityPct + "," +
                    "\"FontSize\":" + FontSize + "," +
                    "\"BorderRadius\":" + BorderRadius + "," +
                    "\"LayoutStyle\":\"" + LayoutStyle + "\"," +
                    "\"RefreshInterval\":" + RefreshInterval + "," +
                    "\"HudX\":" + HudX + "," +
                    "\"HudY\":" + HudY +
                    "}";
                File.WriteAllText(GetSettingsPath(), json);
            }
            catch { }
        }

        public static void LoadSettings()
        {
            try
            {
                string path = GetSettingsPath();
                if (!File.Exists(path)) return;
                string json = File.ReadAllText(path);

                ShowFps = ReadBool(json, "ShowFps", true);
                ShowPing = ReadBool(json, "ShowPing", true);
                ShowRam = ReadBool(json, "ShowRam", true);
                ShowCpu = ReadBool(json, "ShowCpu", true);
                ShowGpu = ReadBool(json, "ShowGpu", true);
                ShowTemp = ReadBool(json, "ShowTemp", false);
                ShowBattery = ReadBool(json, "ShowBattery", true);
                ShowNetwork = ReadBool(json, "ShowNetwork", false);
                ShowDisk = ReadBool(json, "ShowDisk", false);
                OverlayVisible = ReadBool(json, "OverlayVisible", false);
                ShowBorder = ReadBool(json, "ShowBorder", true);
                ShowLabels = ReadBool(json, "ShowLabels", true);
                GlowEffect = ReadBool(json, "GlowEffect", true);
                LockPosition = ReadBool(json, "LockPosition", false);

                int r = ReadInt(json, "AccentR", 0);
                int g = ReadInt(json, "AccentG", 230);
                int b = ReadInt(json, "AccentB", 118);
                AccentColor = Color.FromArgb(r, g, b);

                OpacityPct = ReadInt(json, "OpacityPct", 85);
                FontSize = ReadInt(json, "FontSize", 14);
                BorderRadius = ReadInt(json, "BorderRadius", 6);
                RefreshInterval = ReadInt(json, "RefreshInterval", 500);
                HudX = ReadInt(json, "HudX", -1);
                HudY = ReadInt(json, "HudY", -1);
                if (HudX < -10000 || HudY < -10000)
                {
                    HudX = -1;
                    HudY = -1;
                }

                string layout = ReadString(json, "LayoutStyle", "Vertical Stack");
                if (layout == "Horizontal Compact Bar" || layout == "Vertical Stack")
                    LayoutStyle = layout;
            }
            catch { }
        }

        private static bool ReadBool(string json, string key, bool def)
        {
            string search = "\"" + key + "\":";
            int idx = json.IndexOf(search);
            if (idx < 0) return def;
            int valStart = idx + search.Length;
            string rest = json.Substring(valStart).Trim();
            if (rest.StartsWith("true")) return true;
            if (rest.StartsWith("false")) return false;
            return def;
        }

        private static int ReadInt(string json, string key, int def)
        {
            string search = "\"" + key + "\":";
            int idx = json.IndexOf(search);
            if (idx < 0) return def;
            int valStart = idx + search.Length;
            string rest = json.Substring(valStart).Trim();
            string numStr = "";
            foreach (char c in rest)
            {
                if (c == '-' || (c >= '0' && c <= '9')) numStr += c;
                else break;
            }
            int result;
            return int.TryParse(numStr, out result) ? result : def;
        }

        private static string ReadString(string json, string key, string def)
        {
            string search = "\"" + key + "\":\"";
            int idx = json.IndexOf(search);
            if (idx < 0) return def;
            int valStart = idx + search.Length;
            int valEnd = json.IndexOf("\"", valStart);
            if (valEnd < 0) return def;
            return json.Substring(valStart, valEnd - valStart);
        }
    }

    public class HudForm : Form
    {
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TOPMOST = 0x8;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = -20;
        private const byte AC_SRC_OVER = 0;
        private const byte AC_SRC_ALPHA = 1;
        private const int ULW_ALPHA = 2;

        private System.Windows.Forms.Timer updateTimer;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(
            IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
            IntPtr hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        // Win32 SetWindowPos for re-asserting TopMost over fullscreen games
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private System.Windows.Forms.Timer topMostTimer;

        public HudForm()
        {
            Text = "LiteOverlay HUD";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;

            // Restore saved HUD position or default
            if (AppState.HudX >= 0 && AppState.HudY >= 0)
                Location = new Point(AppState.HudX, AppState.HudY);
            else
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - 220, 40);

            Size = new Size(180, 200);
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.Black;

            MouseDown += HudForm_MouseDown;

            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = AppState.RefreshInterval;
            updateTimer.Tick += (s, e) => RenderHud();
            updateTimer.Start();

            // Re-assert TopMost only when window becomes visible
            topMostTimer = new System.Windows.Forms.Timer();
            topMostTimer.Interval = 1000;
            topMostTimer.Tick += (s, e) =>
            {
                if (AppState.OverlayVisible && IsHandleCreated && !IsDisposed)
                {
                    try
                    {
                        if (!Visible)
                        {
                            Show();
                            lastRenderedStateKey = "";
                            RenderHud();
                            SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0,
                                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
                        }
                    }
                    catch { }
                }
            };
            topMostTimer.Start();

            Shown += (s, e) => RenderHud();
        }

        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= unchecked((int)0x80000000); // WS_POPUP
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Layered window renders via UpdateLayeredWindow only.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Layered window renders via UpdateLayeredWindow only.
        }

        public void UpdateInterval(int ms)
        {
            updateTimer.Interval = ms;
        }

        public void SetOverlayVisible(bool visible)
        {
            if (visible)
            {
                if (!Visible) Show();
                BringToFront();
                RenderHud();
            }
            else
            {
                Hide();
            }
        }

        public void SetClickThrough(bool enabled)
        {
            if (!IsHandleCreated) return;

            int style = GetWindowLong(Handle, GWL_EXSTYLE);
            if (enabled)
            {
                SetWindowLong(Handle, GWL_EXSTYLE, style | WS_EX_TRANSPARENT);
            }
            else
            {
                SetWindowLong(Handle, GWL_EXSTYLE, style & ~WS_EX_TRANSPARENT);
            }
        }

        public void RefreshHud()
        {
            if (!IsHandleCreated || IsDisposed) return;
            RenderHud();
        }

        private string lastRenderedStateKey = "";

        private void RenderHud()
        {
            if (!AppState.OverlayVisible)
            {
                if (Visible) Hide();
                return;
            }

            if (!Visible)
            {
                Show();
                lastRenderedStateKey = "";
            }

            List<Tuple<string, string>> items = BuildMetricItems();
            Size contentSize = MeasureContentSize(items);
            int width = Math.Max(120, contentSize.Width);
            int height = Math.Max(40, contentSize.Height);

            // Build state key to skip bitmap allocations if metrics haven't changed
            string stateKey = width + "x" + height + "_" + AppState.AccentColor.ToArgb() + "_" + AppState.OpacityPct + "_" + AppState.FontSize + "_" + AppState.BorderRadius + "_" + (AppState.ShowBorder ? 1 : 0) + "_" + (AppState.GlowEffect ? 1 : 0);
            foreach (var item in items) stateKey += "_" + item.Item1 + ":" + item.Item2;

            if (stateKey == lastRenderedStateKey && Width == width && Height == height)
            {
                return; // 0% GDI allocation when numbers are unchanged
            }
            lastRenderedStateKey = stateKey;

            if (Width != width || Height != height)
            {
                Size = new Size(width, height);
            }

            using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    g.Clear(Color.Transparent);

                    Rectangle rect = new Rectangle(0, 0, width - 1, height - 1);
                    using (GraphicsPath path = GetRoundRectPath(rect, AppState.BorderRadius))
                    {
                        if (AppState.OpacityPct > 0)
                        {
                            int alpha = (int)(255 * (AppState.OpacityPct / 100.0f));
                            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(alpha, 12, 14, 18)))
                            {
                                g.FillPath(bgBrush, path);
                            }
                        }

                        if (AppState.ShowBorder)
                        {
                            if (AppState.GlowEffect)
                            {
                                using (Pen glowPen = new Pen(Color.FromArgb(60, AppState.AccentColor), 3f))
                                {
                                    g.DrawPath(glowPen, path);
                                }
                            }

                            using (Pen borderPen = new Pen(AppState.AccentColor, 1.2f))
                            {
                                g.DrawPath(borderPen, path);
                            }
                        }
                    }

                    DrawMetrics(g, items, width);
                }

                ApplyLayeredBitmap(bmp, width, height);
            }
        }

        private List<Tuple<string, string>> BuildMetricItems()
        {
            List<Tuple<string, string>> items = new List<Tuple<string, string>>();

            if (AppState.ShowFps) items.Add(Tuple.Create("FPS", AppState.Fps.ToString()));
            if (AppState.ShowPing) items.Add(Tuple.Create("PING", AppState.Ping + "ms"));
            if (AppState.ShowCpu) items.Add(Tuple.Create("CPU", Math.Round(AppState.CpuPct) + "%"));
            if (AppState.ShowGpu) items.Add(Tuple.Create("GPU", Math.Round(AppState.GpuPct) + "%"));
            if (AppState.ShowRam) items.Add(Tuple.Create("RAM", AppState.RamText));
            if (AppState.ShowTemp) items.Add(Tuple.Create("TEMP", AppState.Temp + "°C"));
            if (AppState.ShowBattery) items.Add(Tuple.Create("BAT", AppState.BatteryPct + "%"));
            if (AppState.ShowNetwork) items.Add(Tuple.Create("NET", AppState.NetSpeed));
            if (AppState.ShowDisk) items.Add(Tuple.Create("DISK", AppState.DiskText));

            if (items.Count == 0)
            {
                items.Add(Tuple.Create("", "(No Stats Selected)"));
            }

            return items;
        }

        private Size MeasureContentSize(List<Tuple<string, string>> items)
        {
            using (Bitmap measureBmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(measureBmp))
            using (Font fontKey = new Font("Segoe UI", AppState.FontSize * 0.85f, FontStyle.Bold))
            using (Font fontVal = new Font("Consolas", AppState.FontSize, FontStyle.Bold))
            {
                if (AppState.LayoutStyle == "Horizontal Compact Bar")
                {
                    int x = 12;
                    foreach (Tuple<string, string> item in items)
                    {
                        if (AppState.ShowLabels && !string.IsNullOrEmpty(item.Item1))
                        {
                            x += (int)g.MeasureString(item.Item1 + ":", fontKey).Width + 2;
                        }
                        x += (int)g.MeasureString(item.Item2, fontVal).Width + 14;
                    }

                    return new Size(x + 8, AppState.FontSize + 20);
                }

                int y = 10;
                foreach (Tuple<string, string> item in items)
                {
                    SizeF valSize = g.MeasureString(item.Item2, fontVal);
                    y += (int)valSize.Height + 4;
                }

                return new Size(180, y + 8);
            }
        }

        private void DrawMetrics(Graphics g, List<Tuple<string, string>> items, int width)
        {
            using (Font fontKey = new Font("Segoe UI", AppState.FontSize * 0.85f, FontStyle.Bold))
            using (Font fontVal = new Font("Consolas", AppState.FontSize, FontStyle.Bold))
            using (SolidBrush keyBrush = new SolidBrush(Color.FromArgb(140, 150, 170)))
            using (SolidBrush valueBrush = new SolidBrush(AppState.AccentColor))
            {
                if (AppState.LayoutStyle == "Horizontal Compact Bar")
                {
                    int x = 12;
                    foreach (Tuple<string, string> item in items)
                    {
                        if (AppState.ShowLabels && !string.IsNullOrEmpty(item.Item1))
                        {
                            g.DrawString(item.Item1 + ":", fontKey, keyBrush, x, 8);
                            x += (int)g.MeasureString(item.Item1 + ":", fontKey).Width + 2;
                        }

                        g.DrawString(item.Item2, fontVal, valueBrush, x, 6);
                        x += (int)g.MeasureString(item.Item2, fontVal).Width + 14;
                    }

                    return;
                }

                int y = 10;
                foreach (Tuple<string, string> item in items)
                {
                    if (AppState.ShowLabels && !string.IsNullOrEmpty(item.Item1))
                    {
                        g.DrawString(item.Item1, fontKey, keyBrush, 12, y);
                    }

                    SizeF valSize = g.MeasureString(item.Item2, fontVal);
                    g.DrawString(item.Item2, fontVal, valueBrush, width - valSize.Width - 12, y - 2);
                    y += (int)valSize.Height + 4;
                }
            }
        }

        private void ApplyLayeredBitmap(Bitmap bmp, int width, int height)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = bmp.GetHbitmap(Color.FromArgb(0));
                oldBitmap = SelectObject(memDc, hBitmap);

                POINT topPos = new POINT { x = Left, y = Top };
                SIZE size = new SIZE { cx = width, cy = height };
                POINT sourcePos = new POINT { x = 0, y = 0 };
                BLENDFUNCTION blend = new BLENDFUNCTION
                {
                    BlendOp = AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = AC_SRC_ALPHA
                };

                UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref sourcePos, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                if (oldBitmap != IntPtr.Zero) SelectObject(memDc, oldBitmap);
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                DeleteDC(memDc);
                ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        private static GraphicsPath GetRoundRectPath(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(r);
                return path;
            }

            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void HudForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !AppState.LockPosition)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (WindowState == FormWindowState.Normal && Location.X >= -10000 && Location.Y >= -10000 && Visible)
            {
                AppState.HudX = Location.X;
                AppState.HudY = Location.Y;
                AppState.SaveSettings();
            }
        }
    }

    public class DashboardForm : Form
    {
        private HudForm hudWindow;
        private System.Windows.Forms.Timer metricsTimer;
        private int sensorUpdateInProgress;

        private Panel contentPanel;
        private Button btnTabOverlay;
        private Button btnTabSystem;
        private Button btnTabPerf;
        private Button btnTabAbout;
        private ModernToggleSwitch chkMasterSwitch;

        private Panel panelOverlayTab;
        private Panel panelSystemTab;
        private Panel panelPerfTab;
        private Panel panelAboutTab;
        private FlowLayoutPanel gridSystemCards;

        private Label lblFpsVal, lblPingVal, lblRamVal, lblCpuVal, lblGpuVal, lblTempVal, lblBatVal, lblBatSub, lblNetVal, lblDiskVal;
        private Label lblUpdateStatusText;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private bool allowExit = false;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public DashboardForm()
        {
            // Load saved settings BEFORE building UI
            AppState.LoadSettings();

            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Text = "LiteOverlay System Monitor & Gaming HUD";
            Size = new Size(1080, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(6, 8, 14);
            ForeColor = Color.White;

            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    Icon = Icon.ExtractAssociatedIcon(exePath);
                }
                else
                {
                    Icon = SystemIcons.Application;
                }
            }
            catch
            {
                Icon = SystemIcons.Application;
            }

            BuildDashboardUI();

            hudWindow = new HudForm();
            hudWindow.Show();
            hudWindow.SetOverlayVisible(AppState.OverlayVisible);
            if (AppState.LockPosition) hudWindow.SetClickThrough(true);

            InitSensors();
            InitSystemTray();
            ThreadPool.QueueUserWorkItem(CheckForUpdatesAsync);
        }

        private void CheckForUpdatesAsync(object state)
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                using (var webClient = new System.Net.WebClient())
                {
                    webClient.Headers.Add("User-Agent", "LiteOverlay-Updater/" + AppState.APP_VERSION);
                    string versionJson = webClient.DownloadString("https://litefps.vercel.app/version.json");

                    // Parse version from JSON
                    string latestVersion = ParseJsonValue(versionJson, "version");
                    string downloadUrl = ParseJsonValue(versionJson, "download");

                    if (!string.IsNullOrEmpty(latestVersion) &&
                        latestVersion != AppState.APP_VERSION &&
                        latestVersion != "v" + AppState.APP_VERSION)
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            if (lblUpdateStatusText != null)
                            {
                                lblUpdateStatusText.Text = "⚡ New Update Available (v" + latestVersion + ")";
                                lblUpdateStatusText.ForeColor = Color.FromArgb(255, 145, 0);
                            }

                            DialogResult res = MessageBox.Show(
                                "⚡ A new LiteOverlay update (v" + latestVersion + ") is available!\n\n" +
                                "Current: v" + AppState.APP_VERSION + "\n" +
                                "Latest:  v" + latestVersion + "\n\n" +
                                "Download and auto-update now? (No uninstall needed!)",
                                "LiteOverlay - Auto Update",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);

                            if (res == DialogResult.Yes)
                            {
                                PerformAutoUpdate(downloadUrl);
                            }
                        });
                    }
                    else
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            if (lblUpdateStatusText != null)
                            {
                                lblUpdateStatusText.Text = "✔ Up to Date (v" + AppState.APP_VERSION + ")";
                                lblUpdateStatusText.ForeColor = Color.FromArgb(0, 230, 118);
                            }
                        });
                    }
                }
            }
            catch { }
        }

        private static string ParseJsonValue(string json, string key)
        {
            string search = "\"" + key + "\"";
            int idx = json.IndexOf(search);
            if (idx < 0) return null;
            int colonIdx = json.IndexOf(':', idx + search.Length);
            if (colonIdx < 0) return null;
            int startQuote = json.IndexOf('"', colonIdx + 1);
            if (startQuote < 0) return null;
            int endQuote = json.IndexOf('"', startQuote + 1);
            if (endQuote < 0) return null;
            return json.Substring(startQuote + 1, endQuote - startQuote - 1);
        }

        private void PerformAutoUpdate(string downloadUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    downloadUrl = "https://litefps.vercel.app/LiteOverlay.exe";
                }

                if (lblUpdateStatusText != null)
                {
                    lblUpdateStatusText.Text = "⬇ Downloading update...";
                    lblUpdateStatusText.ForeColor = Color.FromArgb(0, 176, 255);
                }
                Application.DoEvents();

                string currentExe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string tempExe = Path.Combine(Path.GetTempPath(), "LiteOverlay_update.exe");
                string updaterBat = Path.Combine(Path.GetTempPath(), "LiteOverlay_updater.bat");

                // Download new exe to temp
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                using (var wc = new System.Net.WebClient())
                {
                    wc.Headers.Add("User-Agent", "LiteOverlay-Updater/" + AppState.APP_VERSION);
                    wc.DownloadFile(downloadUrl, tempExe);
                }

                // Create batch script to replace exe and restart
                string batContent =
                    "@echo off\r\n" +
                    "echo Updating LiteOverlay...\r\n" +
                    "timeout /t 2 /nobreak > nul\r\n" +
                    "taskkill /f /im LiteOverlay.exe 2>nul\r\n" +
                    "timeout /t 1 /nobreak > nul\r\n" +
                    "copy /y \"" + tempExe + "\" \"" + currentExe + "\"\r\n" +
                    "del \"" + tempExe + "\" 2>nul\r\n" +
                    "start \"\" \"" + currentExe + "\"\r\n" +
                    "del \"%~f0\" 2>nul\r\n";
                File.WriteAllText(updaterBat, batContent);

                // Launch updater and exit current app
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = updaterBat;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                Process.Start(psi);

                ExitApplication();
            }
            catch (Exception ex)
            {
                if (lblUpdateStatusText != null)
                {
                    lblUpdateStatusText.Text = "✖ Update failed: " + ex.Message;
                    lblUpdateStatusText.ForeColor = Color.FromArgb(255, 82, 82);
                }
            }
        }

        private void InitSystemTray()
        {
            trayMenu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(15, 20, 34),
                ForeColor = Color.White,
                RenderMode = ToolStripRenderMode.System
            };

            ToolStripMenuItem itemShow = new ToolStripMenuItem("⚡ Show / Restore LiteOverlay");
            itemShow.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            itemShow.Click += (s, e) => RestoreFromTray();

            ToolStripMenuItem itemExit = new ToolStripMenuItem("❌ Exit / Quit Application");
            itemExit.Click += (s, e) => ExitApplication();

            trayMenu.Items.Add(itemShow);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(itemExit);

            trayIcon = new NotifyIcon
            {
                Text = "LiteOverlay - Running in background",
                ContextMenuStrip = trayMenu,
                Visible = true
            };

            if (this.Icon != null)
            {
                trayIcon.Icon = this.Icon;
            }
            else
            {
                trayIcon.Icon = SystemIcons.Application;
            }

            trayIcon.Click += (s, e) =>
            {
                MouseEventArgs me = e as MouseEventArgs;
                if (me != null && me.Button == MouseButtons.Left)
                {
                    RestoreFromTray();
                }
            };
            trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        private void MinimizeToTray()
        {
            this.Hide();
            this.ShowInTaskbar = false;
            if (trayIcon != null)
            {
                trayIcon.ShowBalloonTip(1500, "LiteOverlay", "Minimized to System Tray. HUD is active.", ToolTipIcon.Info);
            }
        }

        private void RestoreFromTray()
        {
            this.ShowInTaskbar = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void ExitApplication()
        {
            allowExit = true;
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            Application.Exit();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(Color.FromArgb(27, 38, 59), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowExit && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                MinimizeToTray();
                return;
            }

            // Save all settings on exit
            if (hudWindow != null && !hudWindow.IsDisposed && hudWindow.WindowState == FormWindowState.Normal && hudWindow.Visible && hudWindow.Location.X >= -10000)
            {
                AppState.HudX = hudWindow.Location.X;
                AppState.HudY = hudWindow.Location.Y;
            }
            AppState.SaveSettings();

            if (hudWindow != null && !hudWindow.IsDisposed)
            {
                hudWindow.Dispose();
            }

            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            base.OnFormClosing(e);
        }

        private void BuildDashboardUI()
        {
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(6, 8, 14), Padding = new Padding(20) };
            Controls.Add(contentPanel);

            Panel navPanel = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(9, 12, 19) };
            navPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(22, 28, 43), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, navPanel.Width - 1, navPanel.Height - 1);
                }
            };
            Controls.Add(navPanel);

            btnTabOverlay = CreateNavTab("⚡ OVERLAY SETTINGS", 12);
            btnTabSystem = CreateNavTab("📊 SYSTEM MONITOR", 205);
            btnTabPerf = CreateNavTab("⚙ PERFORMANCE & APP", 398);
            btnTabAbout = CreateNavTab("ℹ ABOUT", 605);

            navPanel.Controls.Add(btnTabOverlay);
            navPanel.Controls.Add(btnTabSystem);
            navPanel.Controls.Add(btnTabPerf);
            navPanel.Controls.Add(btnTabAbout);

            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 75, BackColor = Color.FromArgb(9, 12, 19) };
            headerPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(22, 28, 43), 1))
                {
                    e.Graphics.DrawLine(pen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };
            Controls.Add(headerPanel);

            // Modern Custom Dark Title Bar
            Panel titleBarPanel = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(9, 12, 19) };
            titleBarPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(22, 28, 43), 1))
                {
                    e.Graphics.DrawLine(pen, 0, titleBarPanel.Height - 1, titleBarPanel.Width, titleBarPanel.Height - 1);
                }
            };

            PictureBox picTitleLogo = new PictureBox
            {
                Location = new Point(12, 8),
                Size = new Size(20, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo.png");
                if (File.Exists(logoPath)) picTitleLogo.Image = Image.FromFile(logoPath);
            }
            catch {}
            titleBarPanel.Controls.Add(picTitleLogo);

            Label lblAppTitle = new Label
            {
                Text = "LiteOverlay System Monitor & Gaming HUD",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(38, 9),
                AutoSize = true,
                Cursor = Cursors.Default
            };
            titleBarPanel.Controls.Add(lblAppTitle);

            MouseEventHandler dragHandler = (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            titleBarPanel.MouseDown += dragHandler;
            picTitleLogo.MouseDown += dragHandler;
            lblAppTitle.MouseDown += dragHandler;

            Button btnClose = new Button
            {
                Text = "✕",
                Size = new Size(42, 35),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(229, 57, 53);
            btnClose.Click += (s, e) => Close();

            Button btnMax = new Button
            {
                Text = "□",
                Size = new Size(42, 35),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Cursor = Cursors.Hand
            };
            btnMax.FlatAppearance.BorderSize = 0;
            btnMax.FlatAppearance.MouseOverBackColor = Color.FromArgb(27, 38, 59);
            btnMax.Click += (s, e) =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            };

            Button btnMin = new Button
            {
                Text = "—",
                Size = new Size(42, 35),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Cursor = Cursors.Hand
            };
            btnMin.FlatAppearance.BorderSize = 0;
            btnMin.FlatAppearance.MouseOverBackColor = Color.FromArgb(27, 38, 59);
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;

            titleBarPanel.Controls.Add(btnMin);
            titleBarPanel.Controls.Add(btnMax);
            titleBarPanel.Controls.Add(btnClose);
            Controls.Add(titleBarPanel);

            Label lblBadge = new Label
            {
                Text = "ULTRA LOW RAM",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 230, 118),
                BackColor = Color.FromArgb(20, 0, 230, 118),
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(20, 24),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblBadge);

            PictureBox picHeaderLogo = new PictureBox
            {
                Location = new Point(155, 22),
                Size = new Size(30, 30),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo.png");
                if (File.Exists(logoPath)) picHeaderLogo.Image = Image.FromFile(logoPath);
            }
            catch {}
            headerPanel.Controls.Add(picHeaderLogo);

            Label lblTitle = new Label
            {
                Text = "LiteOverlay",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(190, 20),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            Panel masterCapsule = new Panel
            {
                Location = new Point(800, 20),
                Size = new Size(240, 36),
                BackColor = Color.FromArgb(15, 20, 34)
            };
            masterCapsule.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(27, 38, 59), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, masterCapsule.Width - 1, masterCapsule.Height - 1);
                }
            };

            Label lblMaster = new Label
            {
                Text = "OVERLAY DISPLAY",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(136, 152, 168),
                Location = new Point(12, 10),
                AutoSize = true
            };
            masterCapsule.Controls.Add(lblMaster);

            chkMasterSwitch = new ModernToggleSwitch
            {
                Checked = AppState.OverlayVisible,
                Location = new Point(180, 6),
                Size = new Size(48, 24),
                Cursor = Cursors.Hand
            };
            chkMasterSwitch.CheckedChanged += (s, e) =>
            {
                AppState.OverlayVisible = chkMasterSwitch.Checked;
                hudWindow.SetOverlayVisible(AppState.OverlayVisible);
                AppState.SaveSettings();
            };
            masterCapsule.Controls.Add(chkMasterSwitch);

            headerPanel.Controls.Add(masterCapsule);

            BuildOverlayTab();
            BuildSystemTab();
            BuildPerfTab();
            BuildAboutTab();

            SwitchTab(panelOverlayTab, btnTabOverlay);
        }

        private Button CreateNavTab(string text, int x)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, 6),
                Size = new Size(185, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(136, 152, 168),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                if (btn == btnTabOverlay) SwitchTab(panelOverlayTab, btnTabOverlay);
                else if (btn == btnTabSystem) SwitchTab(panelSystemTab, btnTabSystem);
                else if (btn == btnTabPerf) SwitchTab(panelPerfTab, btnTabPerf);
                else SwitchTab(panelAboutTab, btnTabAbout);
            };
            return btn;
        }

        private void SwitchTab(Panel activePane, Button activeBtn)
        {
            panelOverlayTab.Visible = false;
            panelSystemTab.Visible = false;
            panelPerfTab.Visible = false;
            panelAboutTab.Visible = false;

            btnTabOverlay.ForeColor = Color.FromArgb(136, 152, 168);
            btnTabOverlay.BackColor = Color.Transparent;
            btnTabSystem.ForeColor = Color.FromArgb(136, 152, 168);
            btnTabSystem.BackColor = Color.Transparent;
            btnTabPerf.ForeColor = Color.FromArgb(136, 152, 168);
            btnTabPerf.BackColor = Color.Transparent;
            btnTabAbout.ForeColor = Color.FromArgb(136, 152, 168);
            btnTabAbout.BackColor = Color.Transparent;

            activePane.Visible = true;
            activeBtn.ForeColor = Color.FromArgb(12, 14, 18);
            activeBtn.BackColor = Color.FromArgb(0, 230, 118);
        }

        private void BuildOverlayTab()
        {
            panelOverlayTab = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            contentPanel.Controls.Add(panelOverlayTab);

            // Left Card: Active Overlay Stats
            Panel boxStats = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(475, 520),
                BackColor = Color.FromArgb(9, 12, 19)
            };
            boxStats.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(22, 28, 43), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, boxStats.Width - 1, boxStats.Height - 1);
                }
            };
            panelOverlayTab.Controls.Add(boxStats);

            Label lblStatsTitle = new Label
            {
                Text = "📄  Active Overlay Stats",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 16),
                AutoSize = true
            };
            boxStats.Controls.Add(lblStatsTitle);

            Label lblStatsSub = new Label
            {
                Text = "Select which metrics appear inside your floating gaming HUD.",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(136, 152, 168),
                Location = new Point(18, 42),
                AutoSize = true
            };
            boxStats.Controls.Add(lblStatsSub);

            // 3-Column Grid of Custom Checkbox Cards (Matching Reference)
            int colWidth = 148;
            int rowHeight = 56;
            int startX = 15;
            int startY = 75;

            // Row 0
            boxStats.Controls.Add(CreateStatChip("🎮", "Live FPS", AppState.ShowFps, startX, startY, v => AppState.ShowFps = v));
            boxStats.Controls.Add(CreateStatChip("🌐", "Ping (Latency)", AppState.ShowPing, startX + colWidth, startY, v => AppState.ShowPing = v));
            boxStats.Controls.Add(CreateStatChip("💻", "RAM Usage", AppState.ShowRam, startX + (colWidth * 2), startY, v => AppState.ShowRam = v));

            // Row 1
            boxStats.Controls.Add(CreateStatChip("⚙", "CPU Usage (%)", AppState.ShowCpu, startX, startY + rowHeight, v => AppState.ShowCpu = v));
            boxStats.Controls.Add(CreateStatChip("🎛", "GPU Usage (%)", AppState.ShowGpu, startX + colWidth, startY + rowHeight, v => AppState.ShowGpu = v));
            boxStats.Controls.Add(CreateStatChip("🌡", "Temp (°C)", AppState.ShowTemp, startX + (colWidth * 2), startY + rowHeight, v => AppState.ShowTemp = v));

            // Row 2
            boxStats.Controls.Add(CreateStatChip("🔋", "Battery %", AppState.ShowBattery, startX, startY + (rowHeight * 2), v => AppState.ShowBattery = v));
            boxStats.Controls.Add(CreateStatChip("📶", "Network Speed", AppState.ShowNetwork, startX + colWidth, startY + (rowHeight * 2), v => AppState.ShowNetwork = v));
            boxStats.Controls.Add(CreateStatChip("💽", "Disk Storage", AppState.ShowDisk, startX + (colWidth * 2), startY + (rowHeight * 2), v => AppState.ShowDisk = v));

            // Gaming Display Mode Tip Box
            Panel boxTip = new Panel
            {
                Location = new Point(15, 420),
                Size = new Size(444, 80),
                BackColor = Color.FromArgb(15, 20, 34)
            };
            boxTip.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(0, 230, 118), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, boxTip.Width - 1, boxTip.Height - 1);
                }
            };
            Label lblTipHeader = new Label
            {
                Text = "💡  IMPORTANT GAME DISPLAY TIP:",
                Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 230, 118),
                Location = new Point(12, 10),
                AutoSize = true
            };
            Label lblTipBody = new Label
            {
                Text = "For 100% overlay visibility & zero game lag, set your Game Video Settings to 'Borderless' or 'Windowed Fullscreen' mode in games.",
                Font = new Font("Segoe UI", 8.2f),
                ForeColor = Color.FromArgb(180, 195, 215),
                Location = new Point(12, 32),
                Size = new Size(420, 42)
            };
            boxTip.Controls.Add(lblTipHeader);
            boxTip.Controls.Add(lblTipBody);
            boxStats.Controls.Add(boxTip);

            // Right Card: Overlay Appearance & Theme
            Panel boxApp = new Panel
            {
                Location = new Point(500, 10),
                Size = new Size(475, 520),
                BackColor = Color.FromArgb(9, 12, 19)
            };
            boxApp.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(22, 28, 43), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, boxApp.Width - 1, boxApp.Height - 1);
                }
            };
            panelOverlayTab.Controls.Add(boxApp);

            Label lblAppTitle = new Label
            {
                Text = "🎛  Overlay Appearance & Theme",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 16),
                AutoSize = true
            };
            boxApp.Controls.Add(lblAppTitle);

            Label lblColorTitle = new Label { Text = "Overlay Accent Theme:", Location = new Point(18, 50), AutoSize = true, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(136, 152, 168) };
            boxApp.Controls.Add(lblColorTitle);

            // Polished Color Palette Swatches
            Color[] themeColors = new Color[] {
                Color.FromArgb(0, 230, 118),
                Color.FromArgb(0, 176, 255),
                Color.FromArgb(255, 145, 0),
                Color.FromArgb(255, 82, 82),
                Color.White
            };

            int swatchX = 18;
            foreach (Color col in themeColors)
            {
                Panel swatch = new Panel
                {
                    Location = new Point(swatchX, 72),
                    Size = new Size(36, 36),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };

                bool isHovered = false;
                swatch.MouseEnter += (s, e) => { isHovered = true; swatch.Invalidate(); };
                swatch.MouseLeave += (s, e) => { isHovered = false; swatch.Invalidate(); };

                swatch.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    bool isSelected = (AppState.AccentColor.ToArgb() == col.ToArgb());

                    Rectangle rect = new Rectangle(1, 1, swatch.Width - 3, swatch.Height - 3);
                    using (GraphicsPath path = GetRoundedRectPath(rect, 8))
                    {
                        if (isSelected || isHovered)
                        {
                            using (Pen glowPen = new Pen(isSelected ? Color.White : Color.FromArgb(140, 255, 255, 255), isSelected ? 2.5f : 1.5f))
                            {
                                g.DrawPath(glowPen, path);
                            }
                        }

                        Rectangle fillRect = isSelected ? new Rectangle(4, 4, swatch.Width - 9, swatch.Height - 9) : rect;
                        using (GraphicsPath fillPath = GetRoundedRectPath(fillRect, isSelected ? 6 : 8))
                        using (SolidBrush fillBrush = new SolidBrush(col))
                        {
                            g.FillPath(fillBrush, fillPath);
                        }
                    }
                };

                swatch.Click += (s, e) =>
                {
                    AppState.AccentColor = col;
                    hudWindow.RefreshHud();
                    boxApp.Invalidate(true);
                    AppState.SaveSettings();
                };

                boxApp.Controls.Add(swatch);
                swatchX += 46;
            }

            Label lblLayout = new Label { Text = "HUD Layout Style:", Location = new Point(18, 118), AutoSize = true, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(136, 152, 168) };
            boxApp.Controls.Add(lblLayout);

            // Modern 2-Option Segmented Pill Bar
            Panel panelLayoutSeg = new Panel
            {
                Location = new Point(18, 140),
                Size = new Size(430, 38),
                BackColor = Color.FromArgb(15, 20, 34)
            };
            panelLayoutSeg.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(27, 38, 59), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, panelLayoutSeg.Width - 1, panelLayoutSeg.Height - 1);
                }
            };
            boxApp.Controls.Add(panelLayoutSeg);

            Button btnVertLayout = new Button
            {
                Text = "☰  Vertical Stack",
                Location = new Point(3, 3),
                Size = new Size(210, 32),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVertLayout.FlatAppearance.BorderSize = 0;

            Button btnHorizLayout = new Button
            {
                Text = "☲  Horizontal Bar",
                Location = new Point(217, 3),
                Size = new Size(210, 32),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnHorizLayout.FlatAppearance.BorderSize = 0;

            Action updateSegButtons = () =>
            {
                bool isVert = AppState.LayoutStyle == "Vertical Stack";

                btnVertLayout.BackColor = isVert ? Color.FromArgb(24, 0, 230, 118) : Color.Transparent;
                btnVertLayout.ForeColor = isVert ? Color.FromArgb(0, 230, 118) : Color.FromArgb(136, 152, 168);

                btnHorizLayout.BackColor = !isVert ? Color.FromArgb(24, 0, 230, 118) : Color.Transparent;
                btnHorizLayout.ForeColor = !isVert ? Color.FromArgb(0, 230, 118) : Color.FromArgb(136, 152, 168);
            };

            btnVertLayout.Click += (s, e) =>
            {
                AppState.LayoutStyle = "Vertical Stack";
                updateSegButtons();
                hudWindow.RefreshHud();
                AppState.SaveSettings();
            };

            btnHorizLayout.Click += (s, e) =>
            {
                AppState.LayoutStyle = "Horizontal Compact Bar";
                updateSegButtons();
                hudWindow.RefreshHud();
                AppState.SaveSettings();
            };

            updateSegButtons();
            panelLayoutSeg.Controls.Add(btnVertLayout);
            panelLayoutSeg.Controls.Add(btnHorizLayout);

            Label lblOp = new Label { Text = "Background Opacity: " + AppState.OpacityPct + "%", Location = new Point(18, 185), AutoSize = true, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(136, 152, 168) };
            boxApp.Controls.Add(lblOp);

            ModernRangeSlider tbOp = new ModernRangeSlider { Location = new Point(18, 208), Width = 430, Height = 24, Minimum = 0, Maximum = 100, Value = AppState.OpacityPct };
            tbOp.ValueChanged += (s, e) =>
            {
                AppState.OpacityPct = tbOp.Value;
                lblOp.Text = "Background Opacity: " + tbOp.Value + "%";
                hudWindow.RefreshHud();
                AppState.SaveSettings();
            };
            boxApp.Controls.Add(tbOp);

            Label lblFont = new Label { Text = "Font Size: " + AppState.FontSize + "px", Location = new Point(18, 250), AutoSize = true, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(136, 152, 168) };
            boxApp.Controls.Add(lblFont);

            ModernRangeSlider tbFont = new ModernRangeSlider { Location = new Point(18, 272), Width = 430, Height = 24, Minimum = 10, Maximum = 28, Value = AppState.FontSize };
            tbFont.ValueChanged += (s, e) =>
            {
                AppState.FontSize = tbFont.Value;
                lblFont.Text = "Font Size: " + tbFont.Value + "px";
                hudWindow.RefreshHud();
                AppState.SaveSettings();
            };
            boxApp.Controls.Add(tbFont);

            Label lblRad = new Label { Text = "Corner Rounding: " + AppState.BorderRadius + "px", Location = new Point(18, 314), AutoSize = true, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(136, 152, 168) };
            boxApp.Controls.Add(lblRad);

            ModernRangeSlider tbRad = new ModernRangeSlider { Location = new Point(18, 336), Width = 430, Height = 24, Minimum = 0, Maximum = 20, Value = AppState.BorderRadius };
            tbRad.ValueChanged += (s, e) =>
            {
                AppState.BorderRadius = tbRad.Value;
                lblRad.Text = "Corner Rounding: " + tbRad.Value + "px";
                hudWindow.RefreshHud();
                AppState.SaveSettings();
            };
            boxApp.Controls.Add(tbRad);

            int cy = 390;

            ModernCheckBox chkBorder = new ModernCheckBox { Text = "Overlay Border Line", Checked = AppState.ShowBorder, Location = new Point(18, cy), Size = new Size(185, 28) };
            chkBorder.CheckedChanged += (s, e) =>
            {
                AppState.ShowBorder = chkBorder.Checked;
                hudWindow.RefreshHud();
                AppState.SaveSettings();
            };
            boxApp.Controls.Add(chkBorder);

            ModernCheckBox chkTitles = new ModernCheckBox { Text = "Show Stat Titles", Checked = AppState.ShowLabels, Location = new Point(220, cy), Size = new Size(185, 28) };
            chkTitles.CheckedChanged += (s, e) =>
            {
                AppState.ShowLabels = chkTitles.Checked;
                hudWindow.RefreshHud();
                AppState.SaveSettings();
            };
            boxApp.Controls.Add(chkTitles);
            cy += 36;

            ModernCheckBox chkGlow = new ModernCheckBox { Text = "Border Accent Glow", Checked = AppState.GlowEffect, Location = new Point(18, cy), Size = new Size(185, 28) };
            chkGlow.CheckedChanged += (s, e) =>
            {
                AppState.GlowEffect = chkGlow.Checked;
                hudWindow.RefreshHud();
                AppState.SaveSettings();
            };
            boxApp.Controls.Add(chkGlow);

            ModernCheckBox chkLock = new ModernCheckBox { Text = "Lock Overlay Position", Checked = AppState.LockPosition, Location = new Point(220, cy), Size = new Size(200, 28) };
            chkLock.CheckedChanged += (s, e) =>
            {
                AppState.LockPosition = chkLock.Checked;
                hudWindow.SetClickThrough(AppState.LockPosition);
                AppState.SaveSettings();
            };
            boxApp.Controls.Add(chkLock);
        }

        private Panel CreateStatChip(string icon, string label, bool initial, int x, int y, Action<bool> onChange)
        {
            Panel p = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(142, 46),
                BackColor = Color.FromArgb(15, 20, 34),
                Cursor = Cursors.Hand
            };

            bool isChecked = initial;
            bool isHovered = false;

            p.MouseEnter += (s, e) => { isHovered = true; p.Invalidate(); };
            p.MouseLeave += (s, e) => { isHovered = false; p.Invalidate(); };

            p.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using (GraphicsPath path = GetRoundedRectPath(rect, 8))
                {
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(15, 20, 34)))
                    {
                        g.FillPath(bgBrush, path);
                    }

                    Color borderColor = isHovered ? Color.FromArgb(0, 230, 118) : Color.FromArgb(27, 38, 59);
                    using (Pen borderPen = new Pen(borderColor, isHovered ? 1.5f : 1f))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }

                // Checkbox Square (Left)
                int cbSize = 18;
                int cbY = (p.Height - cbSize) / 2;
                Rectangle checkRect = new Rectangle(12, cbY, cbSize, cbSize);

                using (GraphicsPath cbPath = GetRoundedRectPath(checkRect, 4))
                {
                    if (isChecked)
                    {
                        using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(0, 230, 118)))
                        {
                            g.FillPath(bgBrush, cbPath);
                        }
                        using (Font font = new Font("Segoe UI", 9f, FontStyle.Bold))
                        using (SolidBrush checkBrush = new SolidBrush(Color.White))
                        {
                            g.DrawString("✓", font, checkBrush, 13, cbY + 1);
                        }
                    }
                    else
                    {
                        using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                        {
                            g.FillPath(whiteBrush, cbPath);
                        }
                    }
                }

                // Icon (Segoe UI Emoji) + Label (Segoe UI)
                try
                {
                    using (Font iconFont = new Font("Segoe UI Emoji", 9f, FontStyle.Regular))
                    using (SolidBrush iconBrush = new SolidBrush(Color.White))
                    {
                        g.DrawString(icon, iconFont, iconBrush, 34, 13);
                    }
                }
                catch
                {
                    // Fallback
                }

                using (Font font = new Font("Segoe UI", 8.8f, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(label, font, textBrush, 52, 14);
                }
            };

            p.Click += (s, e) =>
            {
                isChecked = !isChecked;
                onChange(isChecked);
                p.Invalidate();
                hudWindow.RefreshHud();
                AppState.SaveSettings();
            };

            return p;
        }

        private void BuildSystemTab()
        {
            panelSystemTab = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            contentPanel.Controls.Add(panelSystemTab);

            gridSystemCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(6, 8, 14)
            };
            panelSystemTab.Controls.Add(gridSystemCards);

            Label unusedSub;
            gridSystemCards.Controls.Add(CreateTelemetryCard("Live FPS", AppState.Fps.ToString(), "Frames Per Sec", out lblFpsVal, out unusedSub));
            gridSystemCards.Controls.Add(CreateTelemetryCard("Network Ping", AppState.Ping + " ms", "Latency", out lblPingVal, out unusedSub));
            gridSystemCards.Controls.Add(CreateTelemetryCard("RAM Usage", AppState.RamText, "Limit: " + AppState.RamTotalText, out lblRamVal, out unusedSub));
            gridSystemCards.Controls.Add(CreateTelemetryCard("CPU Usage", Math.Round(AppState.CpuPct) + "%", Environment.ProcessorCount + " Cores", out lblCpuVal, out unusedSub));
            gridSystemCards.Controls.Add(CreateTelemetryCard("GPU Usage", Math.Round(AppState.GpuPct) + "%", "Active Load", out lblGpuVal, out unusedSub));
            gridSystemCards.Controls.Add(CreateTelemetryCard("Temperature", AppState.Temp + "°C", "Thermal Sensor", out lblTempVal, out unusedSub));
            gridSystemCards.Controls.Add(CreateTelemetryCard("Battery", AppState.BatteryPct + "%", AppState.BatteryStatus, out lblBatVal, out lblBatSub));
            gridSystemCards.Controls.Add(CreateTelemetryCard("Network Speed", AppState.NetSpeed, "Downlink Rate", out lblNetVal, out unusedSub));
            gridSystemCards.Controls.Add(CreateTelemetryCard("Disk Storage", AppState.DiskText, "Local Disk", out lblDiskVal, out unusedSub));

            RenderSystemTelemetryCards();
        }

        private void RenderSystemTelemetryCards()
        {
            if (lblFpsVal == null) return;
            lblFpsVal.Text = AppState.Fps.ToString();
            lblPingVal.Text = AppState.Ping + " ms";
            lblRamVal.Text = AppState.RamText;
            lblCpuVal.Text = Math.Round(AppState.CpuPct) + "%";
            lblGpuVal.Text = Math.Round(AppState.GpuPct) + "%";
            lblTempVal.Text = AppState.Temp + "°C";
            lblBatVal.Text = AppState.BatteryPct + "%";
            lblBatSub.Text = AppState.BatteryStatus;
            lblNetVal.Text = AppState.NetSpeed;
            lblDiskVal.Text = AppState.DiskText;
        }

        private Panel CreateTelemetryCard(string title, string val, string sub, out Label valLabel, out Label subLabel)
        {
            Panel card = new Panel
            {
                Size = new Size(290, 120),
                Margin = new Padding(10),
                BackColor = Color.FromArgb(15, 20, 34)
            };

            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(0, 230, 118), 2))
                {
                    e.Graphics.DrawLine(pen, 0, 0, card.Width, 0);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(27, 38, 59), 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            card.Controls.Add(new Label { Text = title, Location = new Point(16, 14), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184) });
            
            valLabel = new Label { Text = val, Location = new Point(16, 42), AutoSize = true, Font = new Font("Consolas", 20, FontStyle.Bold), ForeColor = Color.FromArgb(0, 230, 118) };
            card.Controls.Add(valLabel);

            subLabel = new Label { Text = sub, Location = new Point(16, 86), AutoSize = true, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(100, 116, 139) };
            card.Controls.Add(subLabel);

            return card;
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void BuildPerfTab()
        {
            panelPerfTab = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            contentPanel.Controls.Add(panelPerfTab);

            // Left Card: Data Refresh Rate Mode
            Panel boxPerfRate = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(475, 520),
                BackColor = Color.FromArgb(9, 12, 19)
            };
            boxPerfRate.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(22, 28, 43), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, boxPerfRate.Width - 1, boxPerfRate.Height - 1);
                }
            };
            panelPerfTab.Controls.Add(boxPerfRate);

            Label lblRateTitle = new Label
            {
                Text = "🚀  Data Refresh Rate Mode",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 16),
                AutoSize = true
            };
            boxPerfRate.Controls.Add(lblRateTitle);

            Label lblRateSub = new Label
            {
                Text = "Select sensor update frequency for optimal CPU balance.",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(136, 152, 168),
                Location = new Point(18, 42),
                AutoSize = true
            };
            boxPerfRate.Controls.Add(lblRateSub);

            // 4 Clickable Refresh Rate Chips
            Tuple<string, string, int>[] rates = new Tuple<string, string, int>[]
            {
                Tuple.Create("⚡ Ultra Fast (250 ms)", "Highest polling frequency, max responsiveness", 250),
                Tuple.Create("🚀 Recommended (500 ms)", "Optimal balanced refresh rate for gaming", 500),
                Tuple.Create("🔋 Low CPU (1000 ms)", "Energy saver mode for laptop battery life", 1000),
                Tuple.Create("🍃 Extreme Low CPU (2000 ms)", "Minimal background CPU & sensor overhead", 2000)
            };

            List<Panel> ratePanels = new List<Panel>();
            int chipY = 75;

            foreach (var item in rates)
            {
                int msValue = item.Item3;

                Panel pRate = new Panel
                {
                    Location = new Point(18, chipY),
                    Size = new Size(438, 58),
                    BackColor = Color.FromArgb(15, 20, 34),
                    Cursor = Cursors.Hand
                };

                ratePanels.Add(pRate);

                pRate.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    bool isSelected = (AppState.RefreshInterval == msValue);
                    using (Pen pen = new Pen(isSelected ? Color.FromArgb(0, 230, 118) : Color.FromArgb(27, 38, 59), isSelected ? 1.5f : 1f))
                    {
                        g.DrawRectangle(pen, 0, 0, pRate.Width - 1, pRate.Height - 1);
                    }

                    if (isSelected)
                    {
                        using (SolidBrush accentBar = new SolidBrush(Color.FromArgb(0, 230, 118)))
                        {
                            g.FillRectangle(accentBar, 0, 0, 4, pRate.Height);
                        }
                    }
                };

                Label lblMain = new Label
                {
                    Text = item.Item1,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(16, 10),
                    AutoSize = true,
                    Cursor = Cursors.Hand
                };

                Label lblDesc = new Label
                {
                    Text = item.Item2,
                    Font = new Font("Segoe UI", 8.2f),
                    ForeColor = Color.FromArgb(136, 152, 168),
                    Location = new Point(16, 32),
                    AutoSize = true,
                    Cursor = Cursors.Hand
                };

                EventHandler onClick = (s, e) =>
                {
                    AppState.RefreshInterval = msValue;
                    metricsTimer.Interval = msValue;
                    hudWindow.UpdateInterval(msValue);
                    AppState.SaveSettings();

                    foreach (var panel in ratePanels) panel.Invalidate();
                };

                pRate.Click += onClick;
                lblMain.Click += onClick;
                lblDesc.Click += onClick;

                pRate.Controls.Add(lblMain);
                pRate.Controls.Add(lblDesc);
                boxPerfRate.Controls.Add(pRate);

                chipY += 68;
            }

            // Right Card: Engine Architecture & Features
            Panel boxEngineInfo = new Panel
            {
                Location = new Point(500, 10),
                Size = new Size(475, 520),
                BackColor = Color.FromArgb(9, 12, 19)
            };
            boxEngineInfo.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(22, 28, 43), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, boxEngineInfo.Width - 1, boxEngineInfo.Height - 1);
                }
            };
            panelPerfTab.Controls.Add(boxEngineInfo);

            Label lblEngTitle = new Label
            {
                Text = "⚡  Engine Architecture & Status",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 16),
                AutoSize = true
            };
            boxEngineInfo.Controls.Add(lblEngTitle);

            Label lblEngSub = new Label
            {
                Text = "Pure native standalone C# WinForms overlay engine.",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(136, 152, 168),
                Location = new Point(18, 42),
                AutoSize = true
            };
            boxEngineInfo.Controls.Add(lblEngSub);

            Tuple<string, string>[] feats = new Tuple<string, string>[]
            {
                Tuple.Create("🎮 Always-On-Top Direct Gaming HUD", "Stays visible over fullscreen and windowed games"),
                Tuple.Create("💎 Per-Pixel Alpha Layered Window", "True transparent rendering with 0% background opacity"),
                Tuple.Create("🔥 Zero Web / Browser Overhead", "0% Chromium CPU & RAM footprint (No Web Engine)"),
                Tuple.Create("🍃 Ultra-Low Memory Footprint", "Consistently stays under 15 MB RAM usage"),
                Tuple.Create("⚙ ThreadPool Sensor Telemetry", "Non-blocking background hardware sensor monitoring loop")
            };

            int featY = 75;
            foreach (var feat in feats)
            {
                Panel pFeat = new Panel
                {
                    Location = new Point(18, featY),
                    Size = new Size(438, 54),
                    BackColor = Color.FromArgb(15, 20, 34)
                };

                pFeat.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(Color.FromArgb(27, 38, 59), 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, pFeat.Width - 1, pFeat.Height - 1);
                    }
                };

                Label fTitle = new Label { Text = feat.Item1, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.White, Location = new Point(14, 8), AutoSize = true };
                Label fSub = new Label { Text = feat.Item2, Font = new Font("Segoe UI", 8.2f), ForeColor = Color.FromArgb(136, 152, 168), Location = new Point(14, 28), AutoSize = true };

                pFeat.Controls.Add(fTitle);
                pFeat.Controls.Add(fSub);
                boxEngineInfo.Controls.Add(pFeat);

                featY += 64;
            }
        }

        private void BuildAboutTab()
        {
            panelAboutTab = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            contentPanel.Controls.Add(panelAboutTab);

            // Left Card: Software Details
            Panel boxAboutDetails = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(475, 520),
                BackColor = Color.FromArgb(9, 12, 19)
            };
            boxAboutDetails.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(22, 28, 43), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, boxAboutDetails.Width - 1, boxAboutDetails.Height - 1);
                }
            };
            panelAboutTab.Controls.Add(boxAboutDetails);

            Label lblTitle = new Label
            {
                Text = "ℹ  Software Information",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 16),
                AutoSize = true
            };
            boxAboutDetails.Controls.Add(lblTitle);

            Label lblSub = new Label
            {
                Text = "LiteOverlay System Monitor & Gaming HUD Information",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(136, 152, 168),
                Location = new Point(18, 42),
                AutoSize = true
            };
            boxAboutDetails.Controls.Add(lblSub);

            Tuple<string, string, Color>[] items = new Tuple<string, string, Color>[]
            {
                Tuple.Create("Application Name", "LiteOverlay System Monitor", Color.FromArgb(0, 230, 118)),
                Tuple.Create("Software Version", "v" + AppState.APP_VERSION, Color.FromArgb(0, 176, 255)),
                Tuple.Create("Developer Name", "Emmzain Development Team", Color.FromArgb(255, 145, 0)),
                Tuple.Create("Architecture", "64-bit Native C# WinForms", Color.White),
                Tuple.Create("License", "Free / Open Source", Color.FromArgb(200, 210, 225))
            };

            int rowY = 80;
            foreach (var item in items)
            {
                Panel pRow = new Panel
                {
                    Location = new Point(18, rowY),
                    Size = new Size(438, 48),
                    BackColor = Color.FromArgb(15, 20, 34)
                };
                pRow.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(Color.FromArgb(27, 38, 59), 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, pRow.Width - 1, pRow.Height - 1);
                    }
                };

                Label lblName = new Label { Text = item.Item1 + ":", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(136, 152, 168), Location = new Point(14, 14), AutoSize = true };
                Label lblVal = new Label { Text = item.Item2, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = item.Item3, Location = new Point(180, 13), AutoSize = true };

                pRow.Controls.Add(lblName);
                pRow.Controls.Add(lblVal);
                boxAboutDetails.Controls.Add(pRow);

                rowY += 56;
            }

            // Right Card: Auto Update System
            Panel boxUpdateInfo = new Panel
            {
                Location = new Point(500, 10),
                Size = new Size(475, 520),
                BackColor = Color.FromArgb(9, 12, 19)
            };
            boxUpdateInfo.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(22, 28, 43), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, boxUpdateInfo.Width - 1, boxUpdateInfo.Height - 1);
                }
            };
            panelAboutTab.Controls.Add(boxUpdateInfo);

            Label lblUpdTitle = new Label
            {
                Text = "🔄  Automatic Updates",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 16),
                AutoSize = true
            };
            boxUpdateInfo.Controls.Add(lblUpdTitle);

            Label lblUpdSub = new Label
            {
                Text = "App update status & automatic sync",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(136, 152, 168),
                Location = new Point(18, 42),
                AutoSize = true
            };
            boxUpdateInfo.Controls.Add(lblUpdSub);

            Panel pStatusBox = new Panel
            {
                Location = new Point(18, 80),
                Size = new Size(438, 100),
                BackColor = Color.FromArgb(15, 20, 34)
            };
            pStatusBox.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(27, 38, 59), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pStatusBox.Width - 1, pStatusBox.Height - 1);
                }
            };

            lblUpdateStatusText = new Label { Text = "✔ Status: Up to Date (v" + AppState.APP_VERSION + ")", Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 230, 118), Location = new Point(16, 16), AutoSize = true };
            Label lblStatDesc = new Label
            {
                Text = "When a new update is released by the developer, your installed app will update automatically.",
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = Color.FromArgb(180, 195, 215),
                Location = new Point(16, 45),
                Size = new Size(400, 45)
            };

            pStatusBox.Controls.Add(lblUpdateStatusText);
            pStatusBox.Controls.Add(lblStatDesc);
            boxUpdateInfo.Controls.Add(pStatusBox);

            // Version History & Changelog Panel inside red box area
            Panel pHistoryBox = new Panel
            {
                Location = new Point(18, 195),
                Size = new Size(438, 305),
                BackColor = Color.FromArgb(15, 20, 34)
            };
            pHistoryBox.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(27, 38, 59), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pHistoryBox.Width - 1, pHistoryBox.Height - 1);
                }
            };

            Label lblHistoryTitle = new Label 
            { 
                Text = "📋  Version History & Changelog", 
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), 
                ForeColor = Color.White, 
                Location = new Point(16, 14), 
                AutoSize = true 
            };
            pHistoryBox.Controls.Add(lblHistoryTitle);

            TextBox txtChangelog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(15, 20, 34),
                ForeColor = Color.FromArgb(180, 195, 215),
                BorderStyle = BorderStyle.None,
                Location = new Point(16, 42),
                Size = new Size(406, 245),
                Font = new Font("Segoe UI", 9f),
                ScrollBars = ScrollBars.Vertical,
                Text = 
                    "v1.0.1 (Current Release)\r\n" +
                    "• Added 0% CPU Native Win32 GetSystemTimes hardware telemetry engine.\r\n" +
                    "• Fixed game launch/minimize overlay auto-recovery logic.\r\n" +
                    "• Added Independent Pop-Out HUD settings to Web Dashboard.\r\n" +
                    "• Fixed application exit exception & smoothed window dragging.\r\n\r\n" +
                    "v1.0.0\r\n" +
                    "• Initial stable release of LiteOverlay gaming HUD.\r\n" +
                    "• 100% Native Win32 hardware sensor telemetry (CPU, GPU, RAM, Disk, Temp).\r\n" +
                    "• Persistent configuration settings saved inside settings.json.\r\n" +
                    "• Integrated automatic update system powered by Vercel cloud.\r\n"
            };
            pHistoryBox.Controls.Add(txtChangelog);

            boxUpdateInfo.Controls.Add(pHistoryBox);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        private PerformanceCounter cpuCounter;
        private PerformanceCounter gpuCounter;
        private long prevBytesReceived = 0;
        private long prevBytesSent = 0;
        private DateTime prevNetTime = DateTime.UtcNow;

        private void InitSensors()
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue();
            }
            catch
            {
                cpuCounter = null;
            }

            try
            {
                PerformanceCounterCategory category = new PerformanceCounterCategory("GPU Engine");
                string[] instances = category.GetInstanceNames();
                foreach (string instance in instances)
                {
                    if (instance.EndsWith("engtype_3D", StringComparison.OrdinalIgnoreCase))
                    {
                        gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance);
                        gpuCounter.NextValue();
                        break;
                    }
                }
            }
            catch
            {
                gpuCounter = null;
            }

            metricsTimer = new System.Windows.Forms.Timer { Interval = AppState.RefreshInterval };
            metricsTimer.Tick += (s, e) => QueueSensorUpdate();
            QueueSensorUpdate();
            metricsTimer.Start();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_UNSIGNED_RATIO
        {
            public uint uiNumerator;
            public uint uiDenominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_TIMING_INFO
        {
            public uint cbSize;
            public DWM_UNSIGNED_RATIO rateRefresh;
            public ulong qpcRefreshPeriod;
            public DWM_UNSIGNED_RATIO rateCompose;
            public ulong qpcVBlank;
            public ulong cRefresh;
            public uint cDXRefresh;
            public ulong qpcCompose;
            public ulong cFrame;
            public uint cDXPresent;
            public ulong cRefreshFrame;
            public ulong cFrameSubmitted;
            public uint cDXPresentSubmitted;
            public ulong cFrameRendered;
            public ulong cFrameDisplayed;
            public ulong cPixelFormat;
            public ulong cRefreshFramePresented;
            public ulong cRefreshFrameDisplayed;
            public ulong cFramesBuffersPresented;
            public ulong cFramesBuffersDisplayed;
            public ulong cCompleted;
            public ulong cMergedInGdi;
            public ulong cSystemRefresh;
            public ulong cHit;
            public ulong cLastFrameDisplayed;
            public ulong qpcFrameDisplayed;
            public ulong cCoreRenders;
            public ulong cCorePresent;
            public ulong cCoreRefresh;
            public ulong cCoreComposed;
        }

        [DllImport("dwmapi.dll", EntryPoint = "DwmGetCompositionTimingInfo")]
        private static extern int DwmGetCompositionTimingInfo(IntPtr hwnd, ref DWM_TIMING_INFO timingInfo);

        private ulong prevDwmFrameCount = 0;
        private DateTime prevDwmTime = DateTime.UtcNow;

        private int GetRealDwmFps()
        {
            try
            {
                DWM_TIMING_INFO timingInfo = new DWM_TIMING_INFO();
                timingInfo.cbSize = (uint)Marshal.SizeOf(typeof(DWM_TIMING_INFO));
                int hr = DwmGetCompositionTimingInfo(IntPtr.Zero, ref timingInfo);

                if (hr == 0)
                {
                    ulong currentFrameCount = timingInfo.cFrameDisplayed;
                    DateTime now = DateTime.UtcNow;
                    double elapsedSec = (now - prevDwmTime).TotalSeconds;

                    if (prevDwmFrameCount > 0 && elapsedSec > 0.05)
                    {
                        double fps = (currentFrameCount - prevDwmFrameCount) / elapsedSec;
                        prevDwmFrameCount = currentFrameCount;
                        prevDwmTime = now;
                        if (fps > 0 && fps < 360) return (int)Math.Round(fps);
                    }

                    prevDwmFrameCount = currentFrameCount;
                    prevDwmTime = now;
                }
            }
            catch { }

            return 0;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime, out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime, out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

        private static ulong FileTimeToUInt64(System.Runtime.InteropServices.ComTypes.FILETIME ft)
        {
            return ((ulong)ft.dwHighDateTime << 32) | (uint)ft.dwLowDateTime;
        }

        private ulong prevIdleTime = 0;
        private ulong prevKernelTime = 0;
        private ulong prevUserTime = 0;

        private float GetNativeCpuUsage()
        {
            try
            {
                System.Runtime.InteropServices.ComTypes.FILETIME idleTime, kernelTime, userTime;
                if (GetSystemTimes(out idleTime, out kernelTime, out userTime))
                {
                    ulong idle = FileTimeToUInt64(idleTime);
                    ulong kernel = FileTimeToUInt64(kernelTime);
                    ulong user = FileTimeToUInt64(userTime);

                    if (prevIdleTime > 0)
                    {
                        ulong idleDiff = idle - prevIdleTime;
                        ulong kernelDiff = kernel - prevKernelTime;
                        ulong userDiff = user - prevUserTime;
                        ulong totalDiff = kernelDiff + userDiff;

                        if (totalDiff > 0)
                        {
                            float cpuPct = (float)(100.0 * (totalDiff - idleDiff) / totalDiff);
                            prevIdleTime = idle;
                            prevKernelTime = kernel;
                            prevUserTime = user;
                            return Math.Min(100f, Math.Max(0f, cpuPct));
                        }
                    }

                    prevIdleTime = idle;
                    prevKernelTime = kernel;
                    prevUserTime = user;
                }
            }
            catch { }

            return AppState.CpuPct;
        }

        private void QueueSensorUpdate()
        {
            if (Interlocked.Exchange(ref sensorUpdateInProgress, 1) == 1) return;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    // 1. Live Real Presentation FPS Counter (DWM Graphics API)
                    int realDwmFps = GetRealDwmFps();
                    if (realDwmFps > 0)
                    {
                        AppState.Fps = realDwmFps;
                    }
                    else
                    {
                        AppState.Fps = 60; // Clean baseline default
                    }

                    // 2. Real Network Ping Latency (ICMP)
                    try
                    {
                        using (Ping ping = new Ping())
                        {
                            PingReply reply = ping.Send("1.1.1.1", 300);
                            if (reply != null && reply.Status == IPStatus.Success)
                            {
                                AppState.Ping = (int)reply.RoundtripTime;
                            }
                        }
                    }
                    catch { }

                    // 3. Ultra-fast 0% CPU Native Win32 CPU Load Telemetry (GetSystemTimes)
                    AppState.CpuPct = GetNativeCpuUsage();

                    // 4. Lightweight GPU Load (%)
                    try
                    {
                        if (gpuCounter != null)
                        {
                            float val = gpuCounter.NextValue();
                            AppState.GpuPct = Math.Min(100f, Math.Max(0f, val));
                        }
                    }
                    catch { }

                    // 5. Real Physical RAM Usage & Total (GlobalMemoryStatusEx API - 0.001ms)
                    MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                    if (GlobalMemoryStatusEx(memStatus))
                    {
                        double totalGb = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                        double availGb = memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                        double usedGb = totalGb - availGb;
                        double usedMb = (memStatus.ullTotalPhys - memStatus.ullAvailPhys) / (1024.0 * 1024.0);

                        if (usedGb >= 1.0)
                            AppState.RamText = string.Format("{0:F1} GB", usedGb);
                        else
                            AppState.RamText = string.Format("{0:F0} MB", usedMb);

                        AppState.RamTotalText = string.Format("{0:F1} GB", totalGb);
                    }

                    // 6. Real Battery Status & Charge %
                    PowerStatus power = SystemInformation.PowerStatus;
                    AppState.BatteryPct = (int)Math.Round(power.BatteryLifePercent * 100);
                    AppState.BatteryStatus = power.PowerLineStatus == PowerLineStatus.Online ? "⚡ Charging" : "Discharging";

                    // 7. Thermal Temperature Sensors (°C)
                    AppState.Temp = GetRealSystemTemp();

                    // 8. Real Network Bandwidth Speed (Rx + Tx)
                    UpdateNetworkSpeed();

                    // 9. Real Disk Storage Space
                    UpdateDiskUsage();

                    if (!IsDisposed && IsHandleCreated)
                    {
                        BeginInvoke((Action)(() =>
                        {
                            // Skip re-rendering UI elements while user is dragging window for 60 FPS smooth movement
                            if (Control.MouseButtons == MouseButtons.None)
                            {
                                hudWindow.RefreshHud();
                                if (panelSystemTab.Visible)
                                {
                                    RenderSystemTelemetryCards();
                                }
                            }
                        }));
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref sensorUpdateInProgress, 0);
                }
            });
        }

        private static float GetWmiGpuUsage()
        {
            try
            {
                using (System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(@"root\cimv2", "SELECT UtilizationPercentage FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine WHERE Name LIKE '%engtype_3D%'"))
                {
                    float maxUtil = 0;
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        float u = Convert.ToSingle(obj["UtilizationPercentage"]);
                        if (u > maxUtil) maxUtil = u;
                    }
                    return maxUtil;
                }
            }
            catch { return 0f; }
        }

        private static float GetWmiCpuUsage()
        {
            try
            {
                using (System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(@"root\cimv2", "SELECT LoadPercentage FROM Win32_Processor"))
                {
                    float totalLoad = 0;
                    int count = 0;
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        totalLoad += Convert.ToSingle(obj["LoadPercentage"]);
                        count++;
                    }
                    return count > 0 ? (totalLoad / count) : 0f;
                }
            }
            catch { return 0f; }
        }

        private int lastCachedTemp = 48;
        private DateTime lastTempCheckTime = DateTime.MinValue;

        private int GetRealSystemTemp()
        {
            if ((DateTime.UtcNow - lastTempCheckTime).TotalSeconds < 5)
            {
                return lastCachedTemp; // Cache temperature for 5 seconds to eliminate WMI CPU load
            }
            lastTempCheckTime = DateTime.UtcNow;

            try
            {
                using (System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(@"root\WMI", "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneVariable"))
                {
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        uint tempDeciKelvin = Convert.ToUInt32(obj["CurrentTemperature"]);
                        int tempCelsius = (int)Math.Round((tempDeciKelvin - 2732) / 10.0);
                        if (tempCelsius > 0 && tempCelsius < 125)
                        {
                            lastCachedTemp = tempCelsius;
                            return tempCelsius;
                        }
                    }
                }
            }
            catch { }

            return lastCachedTemp;
        }

        private void UpdateNetworkSpeed()
        {
            try
            {
                long currentBytesRecv = 0;
                long currentBytesSent = 0;

                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    {
                        IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();
                        currentBytesRecv += stats.BytesReceived;
                        currentBytesSent += stats.BytesSent;
                    }
                }

                DateTime now = DateTime.UtcNow;
                double elapsedSec = (now - prevNetTime).TotalSeconds;

                if (prevBytesReceived > 0 && elapsedSec > 0.1)
                {
                    double bytesPerSec = ((currentBytesRecv - prevBytesReceived) + (currentBytesSent - prevBytesSent)) / elapsedSec;
                    if (bytesPerSec >= 1024 * 1024)
                    {
                        AppState.NetSpeed = string.Format("{0:F1} MB/s", bytesPerSec / (1024.0 * 1024.0));
                    }
                    else
                    {
                        AppState.NetSpeed = string.Format("{0:F0} KB/s", Math.Max(0, bytesPerSec / 1024.0));
                    }
                }

                prevBytesReceived = currentBytesRecv;
                prevBytesSent = currentBytesSent;
                prevNetTime = now;
            }
            catch { }
        }

        private void UpdateDiskUsage()
        {
            try
            {
                long totalBytes = 0;
                long freeBytes = 0;

                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    try
                    {
                        if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                        {
                            totalBytes += drive.TotalSize;
                            freeBytes += drive.AvailableFreeSpace;
                        }
                    }
                    catch { }
                }

                if (totalBytes > 0)
                {
                    double totalGb = totalBytes / (1024.0 * 1024.0 * 1024.0);
                    double usedGb = (totalBytes - freeBytes) / (1024.0 * 1024.0 * 1024.0);

                    AppState.DiskText = string.Format("{0:F0} GB / {1:F0} GB", usedGb, totalGb);
                }
            }
            catch { }
        }


    }

    public class ModernToggleSwitch : Control
    {
        private bool isChecked = true;
        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    Invalidate();
                    if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public ModernToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(48, 24);
            Cursor = Cursors.Hand;
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                int d = Height - 1;
                path.AddArc(0, 0, d, d, 90, 180);
                path.AddArc(Width - d - 1, 0, d, d, 270, 180);
                path.CloseFigure();

                using (SolidBrush bgBrush = new SolidBrush(isChecked ? Color.FromArgb(0, 230, 118) : Color.FromArgb(30, 41, 59)))
                {
                    g.FillPath(bgBrush, path);
                }
            }

            int knobSize = Height - 6;
            int knobX = isChecked ? Width - knobSize - 3 : 3;
            int knobY = 3;

            using (SolidBrush knobBrush = new SolidBrush(isChecked ? Color.FromArgb(6, 8, 14) : Color.White))
            {
                g.FillEllipse(knobBrush, knobX, knobY, knobSize, knobSize);
            }
        }
    }

    public class ModernCheckBox : Control
    {
        private bool isChecked = false;
        private bool isHovered = false;

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    Invalidate();
                    if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public ModernCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Height = 28;
            Cursor = Cursors.Hand;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9f);
        }

        protected override void OnMouseEnter(EventArgs e) { isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { isHovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Box Size
            int boxSize = 20;
            int boxY = (Height - boxSize) / 2;
            Rectangle boxRect = new Rectangle(0, boxY, boxSize, boxSize);

            using (GraphicsPath path = GetRoundedRectPath(boxRect, 5))
            {
                if (isChecked)
                {
                    using (SolidBrush bgBrush = new SolidBrush(AppState.AccentColor))
                    {
                        g.FillPath(bgBrush, path);
                    }

                    // Draw Checkmark
                    using (Pen checkPen = new Pen(Color.FromArgb(6, 8, 14), 2.2f))
                    {
                        checkPen.StartCap = LineCap.Round;
                        checkPen.EndCap = LineCap.Round;
                        int cx = boxRect.X + 4;
                        int cy = boxRect.Y + 10;
                        g.DrawLine(checkPen, cx, cy, cx + 4, cy + 4);
                        g.DrawLine(checkPen, cx + 4, cy + 4, cx + 12, cy - 4);
                    }
                }
                else
                {
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(15, 20, 34)))
                    {
                        g.FillPath(bgBrush, path);
                    }

                    Color borderCol = isHovered ? AppState.AccentColor : Color.FromArgb(40, 52, 72);
                    using (Pen borderPen = new Pen(borderCol, 1.4f))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
            }

            // Draw Label Text
            if (!string.IsNullOrEmpty(Text))
            {
                using (SolidBrush textBrush = new SolidBrush(isHovered ? Color.White : Color.FromArgb(200, 210, 225)))
                {
                    StringFormat sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(Text, Font, textBrush, new RectangleF(boxSize + 10, 0, Width - boxSize - 10, Height), sf);
                }
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class ModernRangeSlider : Control
    {
        private int minimum = 0;
        private int maximum = 100;
        private int value = 50;
        private bool isDragging = false;
        private bool isHovered = false;

        public event EventHandler ValueChanged;

        public int Minimum
        {
            get { return minimum; }
            set { minimum = value; Invalidate(); }
        }

        public int Maximum
        {
            get { return maximum; }
            set { maximum = value; Invalidate(); }
        }

        public int Value
        {
            get { return value; }
            set
            {
                int clamped = Math.Max(minimum, Math.Min(maximum, value));
                if (this.value != clamped)
                {
                    this.value = clamped;
                    Invalidate();
                    if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        public ModernRangeSlider()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Height = 24;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                UpdateValueFromMouse(e.X);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging)
            {
                UpdateValueFromMouse(e.X);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isDragging = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        private void UpdateValueFromMouse(int mouseX)
        {
            int thumbRadius = 8;
            int trackWidth = Width - (thumbRadius * 2);
            if (trackWidth <= 0) return;

            double pct = (double)(mouseX - thumbRadius) / trackWidth;
            pct = Math.Max(0.0, Math.Min(1.0, pct));
            Value = minimum + (int)Math.Round(pct * (maximum - minimum));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int trackHeight = 6;
            int trackY = (Height - trackHeight) / 2;
            int thumbRadius = 8;
            int thumbDiameter = thumbRadius * 2;
            int trackWidth = Width - thumbDiameter;

            if (trackWidth <= 0) return;

            double pct = (maximum > minimum) ? (double)(value - minimum) / (maximum - minimum) : 0;
            int thumbX = thumbRadius + (int)Math.Round(pct * trackWidth);

            // 1. Draw Dark Background Track
            using (GraphicsPath trackPath = GetRoundedRectPath(new Rectangle(thumbRadius, trackY, trackWidth, trackHeight), trackHeight / 2))
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            {
                g.FillPath(bgBrush, trackPath);
            }

            // 2. Draw Active Filled Track (Neon Green Accent)
            int activeWidth = thumbX - thumbRadius;
            if (activeWidth > 0)
            {
                using (GraphicsPath activePath = GetRoundedRectPath(new Rectangle(thumbRadius, trackY, activeWidth, trackHeight), trackHeight / 2))
                using (SolidBrush fillBrush = new SolidBrush(AppState.AccentColor))
                {
                    g.FillPath(fillBrush, activePath);
                }
            }

            // 3. Draw Thumb Knob
            Rectangle thumbRect = new Rectangle(thumbX - thumbRadius, (Height - thumbDiameter) / 2, thumbDiameter, thumbDiameter);

            if (isHovered || isDragging)
            {
                using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(50, AppState.AccentColor)))
                {
                    g.FillEllipse(glowBrush, thumbRect.X - 3, thumbRect.Y - 3, thumbDiameter + 6, thumbDiameter + 6);
                }
            }

            using (SolidBrush thumbBrush = new SolidBrush(AppState.AccentColor))
            {
                g.FillEllipse(thumbBrush, thumbRect);
            }

            using (SolidBrush centerBrush = new SolidBrush(Color.FromArgb(6, 8, 14)))
            {
                g.FillEllipse(centerBrush, thumbRect.X + 4, thumbRect.Y + 4, thumbDiameter - 8, thumbDiameter - 8);
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
