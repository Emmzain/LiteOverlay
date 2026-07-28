// SetupLiteOverlay.exe - Windows Installer
// Features: Folder selection, Desktop Shortcut checkbox, Add/Remove Programs entry
using System;
using System.Diagnostics;
using System.Drawing;
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

    public class InstallerForm : Form
    {
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblFolder;
        private TextBox txtInstallPath;
        private Button btnBrowse;
        private CheckBox chkDesktopShortcut;
        private Button btnInstall;
        private Button btnCancel;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Panel headerPanel;
        private Panel contentPanel;

        private static readonly string[] APP_FILES = new string[] {
            "index.html",
            "styles.css",
            "app.js",
            "tauri_bridge.js",
            "LiteOverlay.exe"
        };

        public InstallerForm()
        {
            this.Text = "LiteOverlay Setup";
            this.Size = new Size(560, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(18, 20, 28);
            this.ForeColor = Color.White;
            this.Icon = SystemIcons.Application;

            BuildUI();
        }

        private void BuildUI()
        {
            // ── Header Panel ──
            headerPanel = new Panel();
            headerPanel.BackColor = Color.FromArgb(24, 28, 38);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 90;
            this.Controls.Add(headerPanel);

            lblTitle = new Label();
            lblTitle.Text = "\u26A1 LiteOverlay Setup Wizard";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 230, 118);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(24, 18);
            headerPanel.Controls.Add(lblTitle);

            lblSubtitle = new Label();
            lblSubtitle.Text = "Ultra Low-End FPS & System Monitor Overlay - v1.0";
            lblSubtitle.Font = new Font("Segoe UI", 9F);
            lblSubtitle.ForeColor = Color.FromArgb(160, 170, 190);
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(26, 56);
            headerPanel.Controls.Add(lblSubtitle);

            // ── Content Panel ──
            contentPanel = new Panel();
            contentPanel.Location = new Point(0, 90);
            contentPanel.Size = new Size(560, 300);
            this.Controls.Add(contentPanel);

            // Installation Folder Label
            lblFolder = new Label();
            lblFolder.Text = "Installation Folder:";
            lblFolder.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFolder.ForeColor = Color.FromArgb(200, 210, 225);
            lblFolder.AutoSize = true;
            lblFolder.Location = new Point(28, 24);
            contentPanel.Controls.Add(lblFolder);

            // Install Path TextBox
            txtInstallPath = new TextBox();
            txtInstallPath.Text = @"C:\LiteOverlay";
            txtInstallPath.Font = new Font("Consolas", 10F);
            txtInstallPath.BackColor = Color.FromArgb(30, 34, 46);
            txtInstallPath.ForeColor = Color.White;
            txtInstallPath.BorderStyle = BorderStyle.FixedSingle;
            txtInstallPath.Location = new Point(30, 52);
            txtInstallPath.Size = new Size(390, 28);
            contentPanel.Controls.Add(txtInstallPath);

            // Browse Button
            btnBrowse = new Button();
            btnBrowse.Text = "Browse...";
            btnBrowse.Font = new Font("Segoe UI", 9F);
            btnBrowse.BackColor = Color.FromArgb(50, 56, 72);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(80, 90, 110);
            btnBrowse.Location = new Point(430, 50);
            btnBrowse.Size = new Size(90, 30);
            btnBrowse.Click += BtnBrowse_Click;
            contentPanel.Controls.Add(btnBrowse);

            // Desktop Shortcut Checkbox
            chkDesktopShortcut = new CheckBox();
            chkDesktopShortcut.Text = "  Create Desktop Shortcut";
            chkDesktopShortcut.Font = new Font("Segoe UI", 10F);
            chkDesktopShortcut.ForeColor = Color.FromArgb(200, 210, 225);
            chkDesktopShortcut.Checked = true;
            chkDesktopShortcut.AutoSize = true;
            chkDesktopShortcut.Location = new Point(30, 100);
            contentPanel.Controls.Add(chkDesktopShortcut);

            // Progress Bar
            progressBar = new ProgressBar();
            progressBar.Location = new Point(30, 155);
            progressBar.Size = new Size(490, 22);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Visible = false;
            contentPanel.Controls.Add(progressBar);

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(0, 230, 118);
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(30, 184);
            contentPanel.Controls.Add(lblStatus);

            // Install Button
            btnInstall = new Button();
            btnInstall.Text = "\u26A1  Install LiteOverlay";
            btnInstall.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnInstall.BackColor = Color.FromArgb(0, 180, 90);
            btnInstall.ForeColor = Color.White;
            btnInstall.FlatStyle = FlatStyle.Flat;
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Location = new Point(30, 225);
            btnInstall.Size = new Size(230, 44);
            btnInstall.Cursor = Cursors.Hand;
            btnInstall.Click += BtnInstall_Click;
            contentPanel.Controls.Add(btnInstall);

            // Cancel Button
            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Font = new Font("Segoe UI", 10F);
            btnCancel.BackColor = Color.FromArgb(50, 56, 72);
            btnCancel.ForeColor = Color.FromArgb(180, 180, 200);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(80, 90, 110);
            btnCancel.Location = new Point(280, 225);
            btnCancel.Size = new Size(120, 44);
            btnCancel.Click += delegate { this.Close(); };
            contentPanel.Controls.Add(btnCancel);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select Installation Folder for LiteOverlay";
            fbd.SelectedPath = txtInstallPath.Text;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtInstallPath.Text = Path.Combine(fbd.SelectedPath, "LiteOverlay");
            }
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            string installDir = txtInstallPath.Text.Trim();
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
                }

                // Step 3: Create Uninstaller batch file
                lblStatus.Text = "Creating uninstaller...";
                Application.DoEvents();
                string uninstallBat = Path.Combine(installDir, "Uninstall.bat");
                string batContent =
                    "@echo off\r\n" +
                    "echo Uninstalling LiteOverlay...\r\n" +
                    "timeout /t 2 /nobreak > nul\r\n" +
                    "reg delete \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\LiteOverlay\" /f 2>nul\r\n" +
                    "del \"%USERPROFILE%\\Desktop\\LiteOverlay.lnk\" 2>nul\r\n" +
                    "cd /d \"%~dp0\\..\"\r\n" +
                    "rmdir /s /q \"" + installDir + "\"\r\n" +
                    "echo LiteOverlay uninstalled successfully!\r\n" +
                    "pause\r\n";
                File.WriteAllText(uninstallBat, batContent);
                progressBar.Value = progressBar.Maximum - 2;

                // Step 4: Register in Add/Remove Programs (HKCU - no admin required)
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
                        key.SetValue("EstimatedSize", 1024); // ~1 MB
                        key.Close();
                    }
                }
                catch (Exception regEx)
                {
                    MessageBox.Show("Registry entry skip: " + regEx.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
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

                        // Use WScript.Shell COM to create .lnk shortcut
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
                    catch (Exception scEx)
                    {
                        MessageBox.Show("Desktop shortcut skip: " + scEx.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                progressBar.Value = progressBar.Maximum;

                lblStatus.Text = "Installation complete!";
                lblStatus.ForeColor = Color.FromArgb(0, 230, 118);

                DialogResult result = MessageBox.Show(
                    "LiteOverlay installed successfully!\n\n" +
                    "Location: " + installDir + "\n\n" +
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
    }
}
