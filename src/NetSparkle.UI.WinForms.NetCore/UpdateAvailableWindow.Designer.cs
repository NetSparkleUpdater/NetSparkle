namespace NetSparkle.UI.WinForms
{
    /// <summary>
    /// The main form
    /// </summary>
    partial class UpdateAvailableWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateAvailableWindow));
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblInfoText = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.skipButton = new System.Windows.Forms.Button();
            this.buttonRemind = new System.Windows.Forms.Button();
            this.updateButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ReleaseNotesBrowser = new System.Windows.Forms.WebBrowser();
            this.imgAppIcon = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            resources.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.AutoEllipsis = true;
            this.lblHeader.Name = "lblHeader";
            // 
            // lblInfoText
            // 
            resources.ApplyResources(this.lblInfoText, "lblInfoText");
            this.lblInfoText.Name = "lblInfoText";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // skipButton
            // 
            resources.ApplyResources(this.skipButton, "skipButton");
            this.skipButton.Name = "skipButton";
            this.skipButton.UseVisualStyleBackColor = true;
            this.skipButton.Click += new System.EventHandler(this.OnSkipButtonClick);
            // 
            // buttonRemind
            // 
            resources.ApplyResources(this.buttonRemind, "buttonRemind");
            this.buttonRemind.Name = "buttonRemind";
            this.buttonRemind.UseVisualStyleBackColor = true;
            this.buttonRemind.Click += new System.EventHandler(this.OnRemindClick);
            // 
            // updateButton
            // 
            resources.ApplyResources(this.updateButton, "updateButton");
            this.updateButton.Name = "updateButton";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.OnUpdateButtonClick);
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.ReleaseNotesBrowser);
            this.panel1.Name = "panel1";
            // 
            // ReleaseNotesBrowser
            // 
            resources.ApplyResources(this.ReleaseNotesBrowser, "ReleaseNotesBrowser");
            this.ReleaseNotesBrowser.IsWebBrowserContextMenuEnabled = false;
            this.ReleaseNotesBrowser.Name = "ReleaseNotesBrowser";
            // 
            // imgAppIcon
            // 
            resources.ApplyResources(this.imgAppIcon, "imgAppIcon");
            this.imgAppIcon.Name = "imgAppIcon";
            this.imgAppIcon.TabStop = false;
            // 
            // UpdateAvailableWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.buttonRemind);
            this.Controls.Add(this.skipButton);
            this.Controls.Add(this.lblInfoText);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.imgAppIcon);
            this.MaximizeBox = false;
            this.Name = "UpdateAvailableWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
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
        private System.Windows.Forms.WebBrowser ReleaseNotesBrowser;
    }
}