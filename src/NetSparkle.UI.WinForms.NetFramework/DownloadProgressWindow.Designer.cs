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
            this.lblHeader = new System.Windows.Forms.Label();
            this.progressDownload = new System.Windows.Forms.ProgressBar();
            this.btnInstallAndReLaunch = new System.Windows.Forms.Button();
            this.imgAppIcon = new System.Windows.Forms.PictureBox();
            this.downloadProgressLbl = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoEllipsis = true;
            resources.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.Name = "lblHeader";
            // 
            // progressDownload
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.progressDownload, 2);
            resources.ApplyResources(this.progressDownload, "progressDownload");
            this.progressDownload.Name = "progressDownload";
            // 
            // btnInstallAndReLaunch
            // 
            resources.ApplyResources(this.btnInstallAndReLaunch, "btnInstallAndReLaunch");
            this.tableLayoutPanel1.SetColumnSpan(this.btnInstallAndReLaunch, 2);
            this.btnInstallAndReLaunch.Name = "btnInstallAndReLaunch";
            this.btnInstallAndReLaunch.UseVisualStyleBackColor = true;
            this.btnInstallAndReLaunch.Click += new System.EventHandler(this.OnInstallAndReLaunchClick);
            // 
            // imgAppIcon
            // 
            resources.ApplyResources(this.imgAppIcon, "imgAppIcon");
            this.imgAppIcon.Name = "imgAppIcon";
            this.imgAppIcon.TabStop = false;
            // 
            // downloadProgressLbl
            // 
            resources.ApplyResources(this.downloadProgressLbl, "downloadProgressLbl");
            this.tableLayoutPanel1.SetColumnSpan(this.downloadProgressLbl, 2);
            this.downloadProgressLbl.Name = "downloadProgressLbl";
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.tableLayoutPanel1.SetColumnSpan(this.buttonCancel, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.imgAppIcon, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblHeader, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnInstallAndReLaunch, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.downloadProgressLbl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.progressDownload, 0, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // DownloadProgressWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.Name = "DownloadProgressWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

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