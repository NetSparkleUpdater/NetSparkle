namespace AppLimit.NetSparkle
{
    partial class NetSparkleDownloadProgress
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetSparkleDownloadProgress));
            this.lblHeader = new System.Windows.Forms.Label();
            this.progressDownload = new System.Windows.Forms.ProgressBar();
            this.btnInstallAndReLaunch = new System.Windows.Forms.Button();
            this.lblSecurityHint = new System.Windows.Forms.Label();
            this.imgAppIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AccessibleDescription = null;
            this.lblHeader.AccessibleName = null;
            resources.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.Name = "lblHeader";
            // 
            // progressDownload
            // 
            this.progressDownload.AccessibleDescription = null;
            this.progressDownload.AccessibleName = null;
            resources.ApplyResources(this.progressDownload, "progressDownload");
            this.progressDownload.BackgroundImage = null;
            this.progressDownload.Font = null;
            this.progressDownload.Name = "progressDownload";
            // 
            // btnInstallAndReLaunch
            // 
            this.btnInstallAndReLaunch.AccessibleDescription = null;
            this.btnInstallAndReLaunch.AccessibleName = null;
            resources.ApplyResources(this.btnInstallAndReLaunch, "btnInstallAndReLaunch");
            this.btnInstallAndReLaunch.BackgroundImage = null;
            this.btnInstallAndReLaunch.Font = null;
            this.btnInstallAndReLaunch.Name = "btnInstallAndReLaunch";
            this.btnInstallAndReLaunch.UseVisualStyleBackColor = true;
            this.btnInstallAndReLaunch.Click += new System.EventHandler(this.btnInstallAndReLaunch_Click);
            // 
            // lblSecurityHint
            // 
            this.lblSecurityHint.AccessibleDescription = null;
            this.lblSecurityHint.AccessibleName = null;
            resources.ApplyResources(this.lblSecurityHint, "lblSecurityHint");
            this.lblSecurityHint.Name = "lblSecurityHint";
            // 
            // imgAppIcon
            // 
            this.imgAppIcon.AccessibleDescription = null;
            this.imgAppIcon.AccessibleName = null;
            resources.ApplyResources(this.imgAppIcon, "imgAppIcon");
            this.imgAppIcon.BackgroundImage = null;
            this.imgAppIcon.Font = null;
            this.imgAppIcon.Image = global::AppLimit.NetSparkle.Properties.Resources.software_update_available1;
            this.imgAppIcon.ImageLocation = null;
            this.imgAppIcon.Name = "imgAppIcon";
            this.imgAppIcon.TabStop = false;
            // 
            // NetSparkleDownloadProgress
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = null;
            this.Controls.Add(this.lblSecurityHint);
            this.Controls.Add(this.btnInstallAndReLaunch);
            this.Controls.Add(this.progressDownload);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.imgAppIcon);
            this.Font = null;
            this.Name = "NetSparkleDownloadProgress";
            this.ShowInTaskbar = false;
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.PictureBox imgAppIcon;
        private System.Windows.Forms.ProgressBar progressDownload;
        private System.Windows.Forms.Button btnInstallAndReLaunch;
        private System.Windows.Forms.Label lblSecurityHint;


    }
}