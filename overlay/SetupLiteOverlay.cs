// SetupLiteOverlay.exe - Premium Dark Theme Windows Installer
// Matches website dark UI with neon green accents
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace LiteOverlaySetup
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerForm());
        }
    }

    // ═══════════════════════════════════════════
    //  Custom Dark Styled Button
    // ═══════════════════════════════════════════
    public class DarkButton : Button
    {
        private Color bgNormal;
        private Color bgHover;
        private Color bgPress;
        private bool isHovered = false;
        private bool isPressed = false;
        private int radius = 8;

        public DarkButton(Color normal, Color hover, Color press)
        {
            bgNormal = normal;
            bgHover = hover;
            bgPress = press;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e) { isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { isHovered = false; isPressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { isPressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { isPressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color bg = isPressed ? bgPress : (isHovered ? bgHover : bgNormal);

            GraphicsPath path = RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), radius);
            g.FillPath(new SolidBrush(bg), path);

            if (isHovered && !isPressed)
            {
                using (Pen glowPen = new Pen(Color.FromArgb(40, 0, 230, 118), 1))
                {
                    g.DrawPath(glowPen, path);
                }
            }

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            g.DrawString(Text, Font, new SolidBrush(ForeColor), new RectangleF(0, 0, Width, Height), sf);
        }

        private GraphicsPath RoundRect(Rectangle r, int rad)
        {
            GraphicsPath gp = new GraphicsPath();
            int d = rad * 2;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }

    // ═══════════════════════════════════════════
    //  Custom Dark Progress Bar
    // ═══════════════════════════════════════════
    public class DarkProgressBar : Control
    {
        private int val = 0;
        private int max = 100;

        public int Value { get { return val; } set { val = Math.Min(value, max); Invalidate(); } }
        public int Maximum { get { return max; } set { max = value; Invalidate(); } }

        public DarkProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            Height = 8;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Track
            GraphicsPath trackPath = RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 4);
            g.FillPath(new SolidBrush(Color.FromArgb(30, 34, 46)), trackPath);

            // Fill
            if (val > 0 && max > 0)
            {
                int fillW = Math.Max(8, (int)((float)val / max * (Width - 1)));
                GraphicsPath fillPath = RoundRect(new Rectangle(0, 0, fillW, Height - 1), 4);
                using (LinearGradientBrush lgb = new LinearGradientBrush(
                    new Point(0, 0), new Point(fillW, 0),
                    Color.FromArgb(0, 180, 90), Color.FromArgb(0, 230, 118)))
                {
                    g.FillPath(lgb, fillPath);
                }
            }
        }

        private GraphicsPath RoundRect(Rectangle r, int rad)
        {
            GraphicsPath gp = new GraphicsPath();
            int d = rad * 2;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }

    // ═══════════════════════════════════════════
    //  Custom Dark CheckBox
    // ═══════════════════════════════════════════
    public class DarkCheckBox : CheckBox
    {
        public DarkCheckBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : Color.FromArgb(18, 20, 28));

            // Checkbox box
            Rectangle boxRect = new Rectangle(0, (Height - 18) / 2, 18, 18);
            GraphicsPath boxPath = RoundRect(boxRect, 4);

            if (Checked)
            {
                g.FillPath(new SolidBrush(Color.FromArgb(0, 200, 100)), boxPath);
                // Checkmark
                using (Pen checkPen = new Pen(Color.White, 2.2f))
                {
                    checkPen.StartCap = LineCap.Round;
                    checkPen.EndCap = LineCap.Round;
                    int cx = boxRect.X + 4;
                    int cy = boxRect.Y + 9;
                    g.DrawLine(checkPen, cx, cy, cx + 3, cy + 3);
                    g.DrawLine(checkPen, cx + 3, cy + 3, cx + 10, cy - 4);
                }
            }
            else
            {
                g.FillPath(new SolidBrush(Color.FromArgb(30, 34, 46)), boxPath);
                g.DrawPath(new Pen(Color.FromArgb(70, 80, 100), 1.2f), boxPath);
            }

            // Text
            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            g.DrawString(Text, Font, new SolidBrush(ForeColor),
                new RectangleF(26, 0, Width - 28, Height), sf);
        }

        private GraphicsPath RoundRect(Rectangle r, int rad)
        {
            GraphicsPath gp = new GraphicsPath();
            int d = rad * 2;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }

    // ═══════════════════════════════════════════
    //  Custom Dark TextBox (path input)
    // ═══════════════════════════════════════════
    public class DarkTextBox : Panel
    {
        public TextBox Inner;

        public DarkTextBox()
        {
            BackColor = Color.FromArgb(26, 30, 40);
            Padding = new Padding(10, 6, 10, 6);
            Inner = new TextBox();
            Inner.Dock = DockStyle.Fill;
            Inner.BackColor = Color.FromArgb(26, 30, 40);
            Inner.ForeColor = Color.FromArgb(220, 225, 235);
            Inner.Font = new Font("Consolas", 10.5f);
            Inner.BorderStyle = BorderStyle.None;
            Controls.Add(Inner);
        }

        public string PathText
        {
            get { return Inner.Text; }
            set { Inner.Text = value; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen borderPen = new Pen(Color.FromArgb(50, 58, 75), 1.4f))
            {
                GraphicsPath path = RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 6);
                g.DrawPath(borderPen, path);
            }
        }

        private GraphicsPath RoundRect(Rectangle r, int rad)
        {
            GraphicsPath gp = new GraphicsPath();
            int d = rad * 2;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }

    // ═══════════════════════════════════════════
    //  Main Installer Form
    // ═══════════════════════════════════════════
    public class InstallerForm : Form
    {
        // ── Colors (match website) ──
        private static readonly Color BG_MAIN = Color.FromArgb(12, 14, 18);
        private static readonly Color BG_HEADER = Color.FromArgb(16, 19, 26);
        private static readonly Color BG_CARD = Color.FromArgb(20, 24, 32);
        private static readonly Color ACCENT = Color.FromArgb(0, 230, 118);
        private static readonly Color ACCENT_DIM = Color.FromArgb(0, 180, 90);
        private static readonly Color TEXT_PRIMARY = Color.FromArgb(225, 230, 240);
        private static readonly Color TEXT_MUTED = Color.FromArgb(120, 135, 160);
        private static readonly Color BORDER_COLOR = Color.FromArgb(36, 42, 56);

        private DarkTextBox txtInstallPath;
        private DarkCheckBox chkDesktopShortcut;
        private DarkButton btnInstall;
        private DarkButton btnCancel;
        private DarkButton btnBrowse;
        private DarkProgressBar progressBar;
        private Label lblStatus;

        private static readonly string[] APP_FILES = new string[] {
            "LiteOverlay.exe"
        };


        public InstallerForm()
        {
            this.Text = "LiteOverlay Setup";
            this.Size = new Size(620, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = BG_MAIN;
            this.ForeColor = TEXT_PRIMARY;
            this.DoubleBuffered = true;
            this.Icon = SystemIcons.Application;

            BuildUI();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // ── Top Header Gradient Bar ──
            using (LinearGradientBrush headerBrush = new LinearGradientBrush(
                new Rectangle(0, 0, Width, 110), BG_HEADER, BG_MAIN, 90f))
            {
                g.FillRectangle(headerBrush, 0, 0, Width, 110);
            }

            // Subtle accent glow line at top
            using (LinearGradientBrush glowBrush = new LinearGradientBrush(
                new Point(0, 0), new Point(Width, 0),
                Color.FromArgb(0, ACCENT), Color.FromArgb(80, ACCENT)))
            {
                g.FillRectangle(glowBrush, 0, 0, Width, 2);
            }

            // ── Lightning bolt icon ──
            using (Font iconFont = new Font("Segoe UI", 28f))
            {
                g.DrawString("\u26A1", iconFont, new SolidBrush(ACCENT), new PointF(30, 16));
            }

            // ── Title ──
            using (Font titleFont = new Font("Segoe UI", 20f, FontStyle.Bold))
            {
                g.DrawString("LiteOverlay", titleFont, new SolidBrush(TEXT_PRIMARY), new PointF(80, 22));
            }

            // ── Subtitle ──
            using (Font subFont = new Font("Segoe UI", 9.5f))
            {
                g.DrawString("Setup Wizard  \u2022  Ultra Low-End System Monitor  \u2022  v1.0",
                    subFont, new SolidBrush(TEXT_MUTED), new PointF(82, 58));
            }

            // ── Version badge ──
            RectangleF badgeRect = new RectangleF(Width - 120, 28, 72, 24);
            GraphicsPath badgePath = RoundRectF(badgeRect, 12);
            g.FillPath(new SolidBrush(Color.FromArgb(30, 0, 230, 118)), badgePath);
            g.DrawPath(new Pen(Color.FromArgb(80, 0, 230, 118), 1f), badgePath);
            using (Font badgeFont = new Font("Segoe UI", 8f, FontStyle.Bold))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString("v1.0.0", badgeFont, new SolidBrush(ACCENT), badgeRect, sf);
            }

            // ── Separator line ──
            using (Pen sepPen = new Pen(BORDER_COLOR, 1f))
            {
                g.DrawLine(sepPen, 30, 108, Width - 30, 108);
            }

            // ── Card background for settings ──
            Rectangle cardRect = new Rectangle(28, 120, Width - 76, 225);
            GraphicsPath cardPath = RoundRect(cardRect, 10);
            g.FillPath(new SolidBrush(BG_CARD), cardPath);
            g.DrawPath(new Pen(BORDER_COLOR, 1f), cardPath);

            // ── "Installation Directory" label ──
            using (Font lblFont = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            {
                g.DrawString("Installation Directory", lblFont, new SolidBrush(TEXT_PRIMARY), new PointF(48, 135));
            }

            using (Font descFont = new Font("Segoe UI", 8.5f))
            {
                g.DrawString("Choose where LiteOverlay will be installed on your computer.",
                    descFont, new SolidBrush(TEXT_MUTED), new PointF(48, 158));
            }

            // ── "Options" section label ──
            using (Font optFont = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            {
                g.DrawString("Options", optFont, new SolidBrush(TEXT_PRIMARY), new PointF(48, 248));
            }
        }

        private void BuildUI()
        {
            // ── Path TextBox ──
            txtInstallPath = new DarkTextBox();
            txtInstallPath.Location = new Point(48, 182);
            txtInstallPath.Size = new Size(390, 34);
            txtInstallPath.PathText = @"C:\LiteOverlay";
            this.Controls.Add(txtInstallPath);

            // ── Browse Button ──
            btnBrowse = new DarkButton(
                Color.FromArgb(36, 42, 56),
                Color.FromArgb(50, 58, 75),
                Color.FromArgb(30, 34, 46));
            btnBrowse.Text = "Browse...";
            btnBrowse.Font = new Font("Segoe UI", 9.5f);
            btnBrowse.ForeColor = TEXT_PRIMARY;
            btnBrowse.Location = new Point(448, 182);
            btnBrowse.Size = new Size(100, 34);
            btnBrowse.Click += BtnBrowse_Click;
            this.Controls.Add(btnBrowse);

            // ── Desktop Shortcut Checkbox ──
            chkDesktopShortcut = new DarkCheckBox();
            chkDesktopShortcut.Text = "Create Desktop Shortcut";
            chkDesktopShortcut.Font = new Font("Segoe UI", 10f);
            chkDesktopShortcut.ForeColor = Color.FromArgb(200, 210, 225);
            chkDesktopShortcut.Checked = true;
            chkDesktopShortcut.Location = new Point(48, 278);
            chkDesktopShortcut.Size = new Size(300, 28);
            this.Controls.Add(chkDesktopShortcut);

            // ── Progress Bar ──
            progressBar = new DarkProgressBar();
            progressBar.Location = new Point(28, 365);
            progressBar.Size = new Size(Width - 76, 8);
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            progressBar.Visible = false;
            this.Controls.Add(progressBar);

            // ── Status Label ──
            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Font = new Font("Segoe UI", 8.5f);
            lblStatus.ForeColor = TEXT_MUTED;
            lblStatus.AutoSize = true;
            lblStatus.BackColor = Color.Transparent;
            lblStatus.Location = new Point(28, 380);
            this.Controls.Add(lblStatus);

            // ── Install Button ──
            btnInstall = new DarkButton(
                Color.FromArgb(0, 180, 90),
                Color.FromArgb(0, 210, 105),
                Color.FromArgb(0, 150, 75));
            btnInstall.Text = "\u26A1  Install LiteOverlay";
            btnInstall.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnInstall.ForeColor = Color.White;
            btnInstall.Location = new Point(28, 405);
            btnInstall.Size = new Size(280, 48);
            btnInstall.Click += BtnInstall_Click;
            this.Controls.Add(btnInstall);

            // ── Cancel Button ──
            btnCancel = new DarkButton(
                Color.FromArgb(36, 42, 56),
                Color.FromArgb(50, 58, 75),
                Color.FromArgb(30, 34, 46));
            btnCancel.Text = "Cancel";
            btnCancel.Font = new Font("Segoe UI", 10.5f);
            btnCancel.ForeColor = Color.FromArgb(180, 180, 200);
            btnCancel.Location = new Point(320, 405);
            btnCancel.Size = new Size(150, 48);
            btnCancel.Click += delegate { this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select Installation Folder for LiteOverlay";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtInstallPath.PathText = Path.Combine(fbd.SelectedPath, "LiteOverlay");
            }
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            string installDir = txtInstallPath.PathText.Trim();
            if (string.IsNullOrEmpty(installDir))
            {
                MessageBox.Show("Please select installation folder.", "LiteOverlay Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sourceDir = AppDomain.CurrentDomain.BaseDirectory;

            btnInstall.Enabled = false;
            btnBrowse.Enabled = false;
            btnCancel.Enabled = false;
            progressBar.Visible = true;
            progressBar.Maximum = APP_FILES.Length + 3;
            progressBar.Value = 0;

            try
            {
                // Step 1: Create install directory
                lblStatus.Text = "Creating installation folder...";
                lblStatus.ForeColor = TEXT_MUTED;
                Application.DoEvents();
                if (!Directory.Exists(installDir))
                {
                    Directory.CreateDirectory(installDir);
                }
                progressBar.Value = 1;

                // Step 2: Copy application files
                int filesCopied = 0;
                foreach (string file in APP_FILES)
                {
                    string srcFile = Path.Combine(sourceDir, file);
                    string destFile = Path.Combine(installDir, file);

                    lblStatus.Text = "Installing: " + file;
                    Application.DoEvents();

                    if (File.Exists(srcFile))
                    {
                        File.Copy(srcFile, destFile, true);
                        filesCopied++;
                    }
                    progressBar.Value = 1 + filesCopied;
                    System.Threading.Thread.Sleep(200); // Visual feedback
                }

                // Step 3: Create Uninstaller
                lblStatus.Text = "Creating uninstaller...";
                Application.DoEvents();
                string uninstallBat = Path.Combine(installDir, "Uninstall.bat");
                string batContent =
                    "@echo off\r\n" +
                    "echo ============================================\r\n" +
                    "echo   Uninstalling LiteOverlay System Monitor\r\n" +
                    "echo ============================================\r\n" +
                    "echo.\r\n" +
                    "timeout /t 2 /nobreak > nul\r\n" +
                    "reg delete \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\LiteOverlay\" /f 2>nul\r\n" +
                    "del \"%USERPROFILE%\\Desktop\\LiteOverlay.lnk\" 2>nul\r\n" +
                    "echo Removing files...\r\n" +
                    "cd /d \"%~dp0\\..\"\r\n" +
                    "rmdir /s /q \"" + installDir + "\"\r\n" +
                    "echo.\r\n" +
                    "echo LiteOverlay has been uninstalled successfully!\r\n" +
                    "echo.\r\n" +
                    "pause\r\n";
                File.WriteAllText(uninstallBat, batContent);
                progressBar.Value = progressBar.Maximum - 2;

                // Step 4: Register in Add/Remove Programs
                lblStatus.Text = "Registering in Windows Programs...";
                Application.DoEvents();
                try
                {
                    string regPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\LiteOverlay";
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(regPath);
                    if (key != null)
                    {
                        key.SetValue("DisplayName", "LiteOverlay System Monitor");
                        key.SetValue("DisplayVersion", "1.0.0");
                        key.SetValue("Publisher", "LiteOverlay");
                        key.SetValue("InstallLocation", installDir);
                        key.SetValue("UninstallString", "\"" + uninstallBat + "\"");
                        key.SetValue("DisplayIcon", Path.Combine(installDir, "LiteOverlay.exe"));
                        key.SetValue("NoModify", 1);
                        key.SetValue("NoRepair", 1);
                        key.SetValue("EstimatedSize", 1024);
                        key.Close();
                    }
                }
                catch { }
                progressBar.Value = progressBar.Maximum - 1;

                // Step 5: Create Desktop Shortcut
                if (chkDesktopShortcut.Checked)
                {
                    lblStatus.Text = "Creating desktop shortcut...";
                    Application.DoEvents();
                    try
                    {
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        string shortcutPath = Path.Combine(desktopPath, "LiteOverlay.lnk");
                        string targetExe = Path.Combine(installDir, "LiteOverlay.exe");

                        Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                        if (shellType != null)
                        {
                            object shell = Activator.CreateInstance(shellType);
                            object shortcut = shellType.InvokeMember("CreateShortcut",
                                BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });

                            Type scType = shortcut.GetType();
                            scType.InvokeMember("TargetPath",
                                BindingFlags.SetProperty, null, shortcut, new object[] { targetExe });
                            scType.InvokeMember("WorkingDirectory",
                                BindingFlags.SetProperty, null, shortcut, new object[] { installDir });
                            scType.InvokeMember("Description",
                                BindingFlags.SetProperty, null, shortcut, new object[] { "LiteOverlay System Monitor" });
                            scType.InvokeMember("Save",
                                BindingFlags.InvokeMethod, null, shortcut, null);
                        }
                    }
                    catch { }
                }
                progressBar.Value = progressBar.Maximum;

                lblStatus.Text = "\u2713 Installation complete!";
                lblStatus.ForeColor = ACCENT;

                DialogResult result = MessageBox.Show(
                    "LiteOverlay installed successfully!\n\n" +
                    "\u2714 Location: " + installDir + "\n" +
                    (chkDesktopShortcut.Checked ? "\u2714 Desktop shortcut created\n" : "") +
                    "\u2714 Registered in Add/Remove Programs\n\n" +
                    "Launch LiteOverlay now?",
                    "LiteOverlay Setup - Complete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    string exePath = Path.Combine(installDir, "LiteOverlay.exe");
                    if (File.Exists(exePath))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = exePath;
                        psi.WorkingDirectory = installDir;
                        Process.Start(psi);
                    }
                }

                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
                lblStatus.ForeColor = Color.FromArgb(255, 82, 82);
                MessageBox.Show("Installation error:\n" + ex.Message, "LiteOverlay Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnInstall.Enabled = true;
                btnBrowse.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        private GraphicsPath RoundRect(Rectangle r, int rad)
        {
            GraphicsPath gp = new GraphicsPath();
            int d = rad * 2;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        private GraphicsPath RoundRectF(RectangleF r, int rad)
        {
            GraphicsPath gp = new GraphicsPath();
            float d = rad * 2f;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}
