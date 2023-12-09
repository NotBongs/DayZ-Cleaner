using System;
using System.Drawing;
using System.IO;
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
        private RadioButton radioButtonCustomRemove; // Added RadioButton for custom removal
        private Button buttonRunScript;
        private ListBox listBoxDeletedFiles;

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
                            System.Diagnostics.Process.Start("https://github.com/NotBongs/DayZ-Cleaner/releases");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while checking for updates: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            labelTitle.ForeColor = System.Drawing.Color.White;
            labelTitle.AutoSize = true;
            labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            labelTitle.Location = new System.Drawing.Point(10, 12);

            buttonExit = new Button();
            buttonExit.Text = "X";
            buttonExit.FlatStyle = FlatStyle.Flat;
            buttonExit.FlatAppearance.BorderSize = 0;
            buttonExit.ForeColor = System.Drawing.Color.White;
            buttonExit.BackColor = System.Drawing.Color.Transparent;
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
            buttonExit.BackColor = Color.Transparent;
        }

        private void ButtonExit_MouseLeave(object sender, EventArgs e)
        {
            buttonExit.ForeColor = Color.White;
            buttonExit.BackColor = Color.Transparent;
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

            radioButtonRemove = new RadioButton();
            radioButtonRemove.AutoSize = true;
            radioButtonRemove.ForeColor = System.Drawing.Color.White;

            radioButtonCustomRemove = new RadioButton();
            radioButtonCustomRemove.AutoSize = true;
            radioButtonCustomRemove.ForeColor = System.Drawing.Color.White;
            radioButtonCustomRemove.Text = "Remove Crash + Script Files";
            radioButtonCustomRemove.Location = new System.Drawing.Point(300, 60);

            buttonRunScript = new Button();
            buttonRunScript.ForeColor = System.Drawing.Color.White;
            buttonRunScript.FlatAppearance.BorderSize = 0;
            buttonRunScript.FlatStyle = FlatStyle.Flat;
            buttonRunScript.BackColor = System.Drawing.Color.DarkSlateBlue;
            buttonRunScript.Text = "Run Script";
            buttonRunScript.Click += ButtonRunScript_Click;

            Button buttonBackup = new Button();
            buttonBackup.Text = "Backup";
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

            radioButtonKeep.Text = "Clear Cache + Keep Map Markers.";
            radioButtonRemove.Text = "Clear Cache + Remove Map Markers.";
            radioButtonKeep.Checked = true;

            radioButtonKeep.Location = new System.Drawing.Point(20, 60);
            radioButtonRemove.Location = new System.Drawing.Point(20, 90);
            buttonRunScript.Location = new System.Drawing.Point(20, 120);
            listBoxDeletedFiles.Location = new System.Drawing.Point(20, 150);
            listBoxDeletedFiles.Size = new System.Drawing.Size(450, 150);
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

            RunScript(keepMapMarkersCache, customRemoveCrashScript);
        }


        private void RunScript(bool keepMapMarkersCache, bool customRemove)
        {
            try
            {
                if (!Directory.Exists(dayZFolder))
                {
                    MessageBox.Show("DayZ folder not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] files = Directory.GetFiles(dayZFolder);
                string[] directories = Directory.GetDirectories(dayZFolder);

                foreach (string file in files)
                {
                    Console.WriteLine($"Processing file: {file}");

                    if (keepMapMarkersCache && Path.GetFileName(file).Equals("MapMarkersCache.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (customRemove)
                    {
                        string fileName = Path.GetFileName(file);
                        if (fileName.StartsWith("crash", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("script", StringComparison.OrdinalIgnoreCase))
                        {

                            bool fileExists = File.Exists(file);
                            if (fileExists)
                            {
                                File.Delete(file);
                                listBoxDeletedFiles.Items.Add($"Deleted file: {file}");
                            }
                            else
                            {
                                listBoxDeletedFiles.Items.Add($"File not found: {file}");
                            }
                        }
                        else
                        {
                            listBoxDeletedFiles.Items.Add($"File name doesn't match criteria: {file}");
                        }
                    }
                    else
                    {
                        File.Delete(file);
                        listBoxDeletedFiles.Items.Add($"Deleted file: {file}");
                    }
                }

                foreach (string directory in directories)
                {
                    if (keepMapMarkersCache && Path.GetFileName(directory).Equals("MapMarkersCache.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    Directory.Delete(directory, true);
                    listBoxDeletedFiles.Items.Add($"Deleted: {directory}");
                }

                string operationMessage = keepMapMarkersCache ? "Script executed to keep MapMarkersCache.json." : "Script executed to remove MapMarkersCache.json.";
                MessageBox.Show(customRemove ? "Custom removal completed." : operationMessage, "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
