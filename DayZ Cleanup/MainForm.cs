using System;
using System.Drawing;
using System.IO;
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
        private Button buttonRunScript;
        private ListBox listBoxDeletedFiles;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomTitleBar();
            this.Icon = Properties.Resources.DZC;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            
            this.BackColor = System.Drawing.ColorTranslator.FromHtml("#191b28");
            this.Text = "DayZ Cleaner";
            dayZFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DayZ");
            InitializeControls();
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
            labelTitle.Location = new System.Drawing.Point(10, 10);

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

            panelTitleBar.Controls.Add(labelTitle);
            panelTitleBar.Controls.Add(buttonExit);
            this.Controls.Add(panelTitleBar);
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
            radioButtonRemove.ForeColor = radioButtonKeep.ForeColor = System.Drawing.Color.White;
            buttonRunScript = new Button();
            buttonRunScript.ForeColor = System.Drawing.Color.White;
            buttonRunScript.FlatAppearance.BorderSize = 0;
            buttonRunScript.FlatStyle = FlatStyle.Flat;
            buttonRunScript.BackColor = System.Drawing.Color.DarkSlateBlue;
            listBoxDeletedFiles = new ListBox();
            listBoxDeletedFiles.BorderStyle = BorderStyle.None;
            listBoxDeletedFiles.ForeColor = radioButtonKeep.ForeColor = System.Drawing.Color.White;
            listBoxDeletedFiles.BackColor = System.Drawing.ColorTranslator.FromHtml("#191b28");


            Button buttonBackup = new Button();
            buttonBackup.Text = "Backup";
            buttonBackup.ForeColor = System.Drawing.Color.White;
            buttonBackup.FlatAppearance.BorderSize = 0;
            buttonBackup.FlatStyle = FlatStyle.Flat;
            buttonBackup.BackColor = System.Drawing.Color.DarkSlateBlue;
            buttonBackup.Click += ButtonBackup_Click;
            buttonBackup.Location = new System.Drawing.Point(150, 120);
            Controls.Add(buttonBackup);

            LinkLabel linkLabelWebsite = new LinkLabel();
            linkLabelWebsite.Text = "Creator";
            linkLabelWebsite.VisitedLinkColor = System.Drawing.Color.DarkSlateBlue;
            linkLabelWebsite.LinkColor = System.Drawing.Color.White;
            linkLabelWebsite.AutoSize = true;
            linkLabelWebsite.Location = new System.Drawing.Point(450, 60);
            linkLabelWebsite.LinkClicked += LinkLabelWebsite_LinkClicked;

            Controls.Add(linkLabelWebsite);


            radioButtonKeep.Text = "Clear Cache + Keep Map Markers.";
            radioButtonRemove.Text = "Clear Cache + Remove Map Markers.";
            radioButtonKeep.Checked = true;

            buttonRunScript.Text = "Run Script";
            buttonRunScript.Click += ButtonRunScript_Click;

            Controls.Add(radioButtonKeep);
            Controls.Add(radioButtonRemove);
            Controls.Add(buttonRunScript);
            Controls.Add(listBoxDeletedFiles);

            radioButtonKeep.Location = new System.Drawing.Point(20, 60);
            radioButtonRemove.Location = new System.Drawing.Point(20, 90);
            buttonRunScript.Location = new System.Drawing.Point(20, 120);
            listBoxDeletedFiles.Location = new System.Drawing.Point(20, 150);
            listBoxDeletedFiles.Size = new System.Drawing.Size(400, 200);
        }

        private void LinkLabelWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://bongsy.rip");
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



        private void RunScript(bool keepMapMarkersCache)
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
                    if (keepMapMarkersCache && Path.GetFileName(file).Equals("MapMarkersCache.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    File.Delete(file);
                    listBoxDeletedFiles.Items.Add($"Deleted file: {file}");
                }

                foreach (string directory in directories)
                {
                    if (keepMapMarkersCache && Path.GetFileName(directory).Equals("MapMarkersCache.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    Directory.Delete(directory, true);
                    listBoxDeletedFiles.Items.Add($"Deleted: {directory}");
                }

                MessageBox.Show(keepMapMarkersCache ? "Script executed to keep MapMarkersCache.json." : "Script executed to remove MapMarkersCache.json.", "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonRunScript_Click(object sender, EventArgs e)
        {
            bool keepMapMarkersCache = radioButtonKeep.Checked;
            RunScript(keepMapMarkersCache);
        }
    }
}
