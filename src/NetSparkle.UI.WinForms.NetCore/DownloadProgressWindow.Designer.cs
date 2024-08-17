namespace NetSparkleUpdater.UI.WinForms
{
    /// <summary>
    /// A progress bar
    /// </summary>
    partial class DownloadProgressWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DownloadProgressWindow));
            lblHeader = new System.Windows.Forms.Label();
            progressDownload = new System.Windows.Forms.ProgressBar();
            btnInstallAndReLaunch = new System.Windows.Forms.Button();
            imgAppIcon = new System.Windows.Forms.PictureBox();
            downloadProgressLbl = new System.Windows.Forms.Label();
            buttonCancel = new System.Windows.Forms.Button();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)imgAppIcon).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblHeader
            // 
            lblHeader.AutoEllipsis = true;
            resources.ApplyResources(lblHeader, "lblHeader");
            lblHeader.Name = "lblHeader";
            // 
            // progressDownload
            // 
            tableLayoutPanel1.SetColumnSpan(progressDownload, 2);
            resources.ApplyResources(progressDownload, "progressDownload");
            progressDownload.Name = "progressDownload";
            progressDownload.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // btnInstallAndReLaunch
            // 
            resources.ApplyResources(btnInstallAndReLaunch, "btnInstallAndReLaunch");
            tableLayoutPanel1.SetColumnSpan(btnInstallAndReLaunch, 2);
            btnInstallAndReLaunch.Name = "btnInstallAndReLaunch";
            btnInstallAndReLaunch.UseVisualStyleBackColor = true;
            btnInstallAndReLaunch.Click += OnInstallAndReLaunchClick;
            // 
            // imgAppIcon
            // 
            resources.ApplyResources(imgAppIcon, "imgAppIcon");
            imgAppIcon.Name = "imgAppIcon";
            imgAppIcon.TabStop = false;
            // 
            // downloadProgressLbl
            // 
            resources.ApplyResources(downloadProgressLbl, "downloadProgressLbl");
            tableLayoutPanel1.SetColumnSpan(downloadProgressLbl, 2);
            downloadProgressLbl.Name = "downloadProgressLbl";
            // 
            // buttonCancel
            // 
            resources.ApplyResources(buttonCancel, "buttonCancel");
            tableLayoutPanel1.SetColumnSpan(buttonCancel, 2);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
            tableLayoutPanel1.Controls.Add(lblHeader, 1, 0);
            tableLayoutPanel1.Controls.Add(buttonCancel, 0, 4);
            tableLayoutPanel1.Controls.Add(imgAppIcon, 0, 0);
            tableLayoutPanel1.Controls.Add(progressDownload, 0, 2);
            tableLayoutPanel1.Controls.Add(btnInstallAndReLaunch, 0, 3);
            tableLayoutPanel1.Controls.Add(downloadProgressLbl, 0, 1);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // DownloadProgressWindow
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            Controls.Add(tableLayoutPanel1);
            MaximizeBox = false;
            Name = "DownloadProgressWindow";
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            ((System.ComponentModel.ISupportInitialize)imgAppIcon).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.ProgressBar progressDownload;
        private System.Windows.Forms.Button btnInstallAndReLaunch;
        private System.Windows.Forms.PictureBox imgAppIcon;
        private System.Windows.Forms.Label downloadProgressLbl;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}