using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DayZ_Cleanup
{
    public partial class MainForm : Form
    {
        private Panel panelTitleBar;
        private Label labelTitle;
        private Button buttonExit;
        private string dayZFolder;
        private RadioButton radioButtonKeep;
        private RadioButton radioButtonRemove;
        private RadioButton radioButtonCustomRemove;
        private Button buttonRunScript;
        private ListBox listBoxDeletedFiles;
        private Label labelTotalDeletedSize;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private const int WM_VSCROLL = 0x115;
        private const int SB_BOTTOM = 7;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomTitleBar();
            CheckForUpdates();
            this.Icon = Properties.Resources.DZC;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = System.Drawing.ColorTranslator.FromHtml("#191b28");
            this.Text = "DayZ Cleaner";
            dayZFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DayZ");
            InitializeControls();

        }

        private void CheckForUpdates()
        {
            try
            {
                string versionCheckUrl = "https://raw.githubusercontent.com/NotBongs/DayZ-Cleaner/main/Version.txt";

                using (WebClient webClient = new WebClient())
                {
                    string latestVersionStr = webClient.DownloadString(versionCheckUrl);
                    Version latestVersion = new Version(latestVersionStr);
                    Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                    if (latestVersion > currentVersion)
                    {
                        DialogResult dialogResult = MessageBox.Show(
                            $"A new version ({latestVersion}) is available! Do you want to update?",
                            "Update Available",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (dialogResult == DialogResult.Yes)
                        {
                            // Launch the updater application
                            StartUpdater();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while checking for updates: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartUpdater()
        {
            try
            {
                // Get the directory of the current executable
                string currentDirectory = Path.GetDirectoryName(Application.ExecutablePath);

                // Path to the updater application in the same directory
                string updaterPath = Path.Combine(currentDirectory, "DayZ Cleaner Updater.exe");

                if (File.Exists(updaterPath))
                {
                    // Start the updater application
                    Process.Start(updaterPath);

                    // Exit the current application
                    Environment.Exit(0);
                }
                else
                {
                    MessageBox.Show("Updater application not found in the same directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while starting the updater: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeCustomTitleBar()
        {
            panelTitleBar = new Panel();
            panelTitleBar.BackColor = System.Drawing.Color.DarkSlateBlue;
            panelTitleBar.Dock = DockStyle.Top;
            panelTitleBar.Height = 40;
            panelTitleBar.MouseDown += PanelTitleBar_MouseDown;

            labelTitle = new Label();
            labelTitle.Text = "DayZ Cleaner";
            labelTitle.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            labelTitle.ForeColor = System.Drawing.Color.White;
            labelTitle.AutoSize = true;
            labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            labelTitle.Location = new System.Drawing.Point(12, 9);

            buttonExit = new Button();
            buttonExit.Cursor = Cursors.Hand;
            buttonExit.Text = "X";
            buttonExit.FlatStyle = FlatStyle.Flat;
            buttonExit.FlatAppearance.BorderSize = 0;
            buttonExit.ForeColor = System.Drawing.Color.White;
            buttonExit.BackColor = System.Drawing.Color.DarkSlateBlue;
            buttonExit.Size = new System.Drawing.Size(40, 40);
            buttonExit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            buttonExit.Click += ButtonExit_Click;
            buttonExit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExit.Location = new System.Drawing.Point(panelTitleBar.Width - buttonExit.Width, 0);
            buttonExit.MouseHover += ButtonExit_MouseHover;
            buttonExit.MouseLeave += ButtonExit_MouseLeave;

            panelTitleBar.Controls.Add(labelTitle);
            panelTitleBar.Controls.Add(buttonExit);
            this.Controls.Add(panelTitleBar);
        }

        private void ButtonExit_MouseHover(object sender, EventArgs e)
        {
            buttonExit.ForeColor = Color.Red;
            buttonExit.BackColor = Color.DarkSlateBlue;
        }

        private void ButtonExit_MouseLeave(object sender, EventArgs e)
        {
            buttonExit.ForeColor = Color.White;
            buttonExit.BackColor = Color.DarkSlateBlue;
        }

        private void PanelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InitializeControls()
        {
            radioButtonKeep = new RadioButton();
            radioButtonKeep.AutoSize = true;
            radioButtonKeep.ForeColor = System.Drawing.Color.White;

            labelTotalDeletedSize = new Label();
            labelTotalDeletedSize.AutoSize = true;
            labelTotalDeletedSize.ForeColor = System.Drawing.Color.White;
            labelTotalDeletedSize.Text = "Total Size Deleted: 0 Bytes";
            labelTotalDeletedSize.Location = new System.Drawing.Point(200, 125);
            Controls.Add(labelTotalDeletedSize);

            radioButtonRemove = new RadioButton();
            radioButtonRemove.AutoSize = true;
            radioButtonRemove.ForeColor = System.Drawing.Color.White;

            radioButtonCustomRemove = new RadioButton();
            radioButtonCustomRemove.Cursor = Cursors.Hand;
            radioButtonCustomRemove.AutoSize = true;
            radioButtonCustomRemove.ForeColor = System.Drawing.Color.White;
            radioButtonCustomRemove.Text = "Remove Crash + Script Files";
            radioButtonCustomRemove.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            radioButtonCustomRemove.Location = new System.Drawing.Point(300, 60);

            buttonRunScript = new Button();
            buttonRunScript.Cursor = Cursors.Hand;
            buttonRunScript.ForeColor = System.Drawing.Color.White;
            buttonRunScript.FlatAppearance.BorderSize = 0;
            buttonRunScript.FlatStyle = FlatStyle.Flat;
            buttonRunScript.BackColor = System.Drawing.Color.DarkSlateBlue;
            buttonRunScript.Text = "Run Script";
            buttonRunScript.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            buttonRunScript.Click += ButtonRunScript_Click;

            Button openFolderButton = new Button();
            openFolderButton.Cursor = Cursors.Hand;
            openFolderButton.Text = "Open Directory";
            openFolderButton.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            openFolderButton.AutoSize = true;
            openFolderButton.ForeColor = System.Drawing.Color.White;
            openFolderButton.BackColor = System.Drawing.Color.DarkSlateBlue;
            openFolderButton.FlatStyle = FlatStyle.Flat;
            openFolderButton.FlatAppearance.BorderSize = 0;
            openFolderButton.Location = new System.Drawing.Point(400, 120);
            openFolderButton.Click += OpenFolderButton_Click;

            Controls.Add(openFolderButton);

            Button buttonBackup = new Button();
            buttonBackup.Cursor = Cursors.Hand;
            buttonBackup.Text = "Backup";
            buttonBackup.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            buttonBackup.ForeColor = System.Drawing.Color.White;
            buttonBackup.FlatAppearance.BorderSize = 0;
            buttonBackup.FlatStyle = FlatStyle.Flat;
            buttonBackup.BackColor = System.Drawing.Color.DarkSlateBlue;
            buttonBackup.Click += ButtonBackup_Click;
            buttonBackup.Location = new System.Drawing.Point(120, 120);
            Controls.Add(buttonBackup);

            listBoxDeletedFiles = new ListBox();
            listBoxDeletedFiles.BorderStyle = BorderStyle.None;
            listBoxDeletedFiles.ForeColor = System.Drawing.Color.White;
            listBoxDeletedFiles.BackColor = System.Drawing.ColorTranslator.FromHtml("#191b28");
            listBoxDeletedFiles.SelectedIndexChanged += ListBoxDeletedFiles_SelectedIndexChanged;

            Controls.Add(radioButtonKeep);
            Controls.Add(radioButtonRemove);
            Controls.Add(radioButtonCustomRemove);
            Controls.Add(buttonRunScript);
            Controls.Add(listBoxDeletedFiles);

            radioButtonKeep.Text = "Clear Cache + Keep Team Settings";
            radioButtonKeep.Cursor = Cursors.Hand;
            radioButtonKeep.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            radioButtonRemove.Text = "Purge All Cache";
            radioButtonRemove.Cursor = Cursors.Hand;
            radioButtonRemove.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            radioButtonKeep.Checked = true;

            radioButtonKeep.Location = new System.Drawing.Point(20, 60);
            radioButtonRemove.Location = new System.Drawing.Point(20, 90);
            buttonRunScript.Location = new System.Drawing.Point(20, 120);
            listBoxDeletedFiles.Location = new System.Drawing.Point(20, 150);
            listBoxDeletedFiles.Size = new System.Drawing.Size(450, 150);
        }

        private void OpenFolderButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Directory.Exists(dayZFolder))
                {
                    System.Diagnostics.Process.Start(dayZFolder);
                }
                else
                {
                    MessageBox.Show("DayZ folder not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening directory: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonBackup_Click(object sender, EventArgs e)
        {
            try
            {
                string sourceFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DayZ");

                if (!Directory.Exists(sourceFolderPath))
                {
                    MessageBox.Show("Source folder not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string backupFolderName = "Backup_" + DateTime.Now.ToString("MM-dd-yy");
                string backupFolderPath = Path.Combine(desktopPath, backupFolderName);

                Directory.CreateDirectory(backupFolderPath);

                CopyAll(sourceFolderPath, backupFolderPath);

                MessageBox.Show("Backup completed successfully!", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during the backup process: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CopyAll(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetDir, fileName);
                File.Copy(file, destFile, true);
                listBoxDeletedFiles.Items.Add($"Copied file '{fileName}' to '{new DirectoryInfo(targetDir).Name}'");
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string folderName = new DirectoryInfo(subDir).Name;
                string destFolder = Path.Combine(targetDir, folderName);
                CopyAll(subDir, destFolder);
                listBoxDeletedFiles.Items.Add($"Copied folder '{folderName}' to '{new DirectoryInfo(targetDir).Name}'");
            }
        }

        private void ListBoxDeletedFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxDeletedFiles.TopIndex = listBoxDeletedFiles.Items.Count - 1;
        }


        private void ButtonRunScript_Click(object sender, EventArgs e)
        {
            bool keepMapMarkersCache = radioButtonKeep.Checked;
            bool customRemoveCrashScript = radioButtonCustomRemove.Checked;
            bool removeEverything = radioButtonRemove.Checked;

            RunScript(keepMapMarkersCache, customRemoveCrashScript, removeEverything);
        }

        private void RunScript(bool keepMapMarkersCache, bool customRemove, bool removeEverything)
        {
            try
            {
                if (!Directory.Exists(dayZFolder))
                {
                    MessageBox.Show("DayZ folder not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                long totalDeletedSize = 0;

                if (keepMapMarkersCache)
                {
                    string[] filesToKeep = { "LBmaster" };

                    string[] files = Directory.GetFiles(dayZFolder);
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        if (!filesToKeep.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                        {
                            if (File.Exists(file))
                            {
                                FileInfo fileInfo = new FileInfo(file);
                                totalDeletedSize += fileInfo.Length;

                                File.Delete(file);
                                listBoxDeletedFiles.Items.Add($"Deleted file: {file}");
                            }
                            else
                            {
                                listBoxDeletedFiles.Items.Add($"Skipped: {file} (not a file)");
                            }
                        }
                    }

                    string[] folders = Directory.GetDirectories(dayZFolder);
                    foreach (string folder in folders)
                    {
                        string folderName = Path.GetFileName(folder);
                        if (!filesToKeep.Contains(folderName, StringComparer.OrdinalIgnoreCase))
                        {
                            Directory.Delete(folder, true);
                            listBoxDeletedFiles.Items.Add($"Deleted folder: {folder}");
                        }
                    }
                }
                else if (customRemove)
                {
                    string[] files = Directory.GetFiles(dayZFolder);
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        if (fileName.StartsWith("crash", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("script", StringComparison.OrdinalIgnoreCase))
                        {
                            if (File.Exists(file))
                            {
                                FileInfo fileInfo = new FileInfo(file);
                                totalDeletedSize += fileInfo.Length;

                                File.Delete(file);
                                listBoxDeletedFiles.Items.Add($"Deleted file: {file}");
                            }
                            else
                            {
                                listBoxDeletedFiles.Items.Add($"Skipped: {file} (not a file)");
                            }
                        }
                    }
                }
                else if (removeEverything)
                {
                    string[] files = Directory.GetFiles(dayZFolder);
                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            totalDeletedSize += fileInfo.Length;

                            File.Delete(file);
                            listBoxDeletedFiles.Items.Add($"Deleted file: {file}");
                        }
                        else
                        {
                            listBoxDeletedFiles.Items.Add($"Skipped: {file} (not a file)");
                        }
                    }

                    string[] folders = Directory.GetDirectories(dayZFolder);
                    foreach (string folder in folders)
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(folder);
                        totalDeletedSize += DirSize(dirInfo);

                        Directory.Delete(folder, true);
                        listBoxDeletedFiles.Items.Add($"Deleted folder: {folder}");
                    }
                }

                string operationMessage = keepMapMarkersCache ? "Successfully Removed Cache & Kept Team Settings"
                    : customRemove ? "Successfully Removed Crash & Script Files"
                    : removeEverything ? "Successfully Purged All"
                    : "Success";

                string totalSizeMessage = GetFormattedSize(totalDeletedSize);

                labelTotalDeletedSize.Text = $"Total Size Deleted: {totalSizeMessage}";
                labelTotalDeletedSize.Font = new Font("Segoe UI", 8, FontStyle.Regular);

                MessageBox.Show(operationMessage, "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during script execution: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private long CalculateTotalDeletedSize(string folderPath)
        {
            long size = 0;

            DirectoryInfo directory = new DirectoryInfo(folderPath);

            foreach (FileInfo file in directory.GetFiles("*", SearchOption.AllDirectories))
            {
                size += file.Length;
            }

            return size;
        }

        private long DirSize(DirectoryInfo d)
        {
            long size = 0;
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        private string GetFormattedSize(long bytes)
        {
            string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }

            return $"{bytes:0.##} {sizes[order]}";
        }
    }
}