namespace AppLimit.NetSparkle
{
    partial class NetSparkleForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetSparkleForm));
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblInfoText = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.skipButton = new System.Windows.Forms.Button();
            this.buttonRemind = new System.Windows.Forms.Button();
            this.updateButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.NetSparkleBrowser = new System.Windows.Forms.WebBrowser();
            this.imgAppIcon = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
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
            // lblInfoText
            // 
            this.lblInfoText.AccessibleDescription = null;
            this.lblInfoText.AccessibleName = null;
            resources.ApplyResources(this.lblInfoText, "lblInfoText");
            this.lblInfoText.Name = "lblInfoText";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // skipButton
            // 
            this.skipButton.AccessibleDescription = null;
            this.skipButton.AccessibleName = null;
            resources.ApplyResources(this.skipButton, "skipButton");
            this.skipButton.BackgroundImage = null;
            this.skipButton.Font = null;
            this.skipButton.Name = "skipButton";
            this.skipButton.UseVisualStyleBackColor = true;
            this.skipButton.Click += new System.EventHandler(this.skipButton_Click);
            // 
            // buttonRemind
            // 
            this.buttonRemind.AccessibleDescription = null;
            this.buttonRemind.AccessibleName = null;
            resources.ApplyResources(this.buttonRemind, "buttonRemind");
            this.buttonRemind.BackgroundImage = null;
            this.buttonRemind.Font = null;
            this.buttonRemind.Name = "buttonRemind";
            this.buttonRemind.UseVisualStyleBackColor = true;
            this.buttonRemind.Click += new System.EventHandler(this.buttonRemind_Click);
            // 
            // updateButton
            // 
            this.updateButton.AccessibleDescription = null;
            this.updateButton.AccessibleName = null;
            resources.ApplyResources(this.updateButton, "updateButton");
            this.updateButton.BackgroundImage = null;
            this.updateButton.Font = null;
            this.updateButton.Name = "updateButton";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.NetSparkleBrowser);
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // NetSparkleBrowser
            // 
            this.NetSparkleBrowser.AccessibleDescription = null;
            this.NetSparkleBrowser.AccessibleName = null;
            resources.ApplyResources(this.NetSparkleBrowser, "NetSparkleBrowser");
            this.NetSparkleBrowser.IsWebBrowserContextMenuEnabled = false;
            this.NetSparkleBrowser.MinimumSize = new System.Drawing.Size(20, 28);
            this.NetSparkleBrowser.Name = "NetSparkleBrowser";
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
            // NetSparkleForm
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.buttonRemind);
            this.Controls.Add(this.skipButton);
            this.Controls.Add(this.lblInfoText);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.imgAppIcon);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetSparkleForm";
            this.ShowInTaskbar = false;
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox imgAppIcon;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblInfoText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button skipButton;
        private System.Windows.Forms.Button buttonRemind;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.WebBrowser NetSparkleBrowser;
    }
}