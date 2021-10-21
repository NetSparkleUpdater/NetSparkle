namespace NetSparkleUpdater.Samples.NetFramework.WinForms
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.checkForUpdatesTimer = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.AppBackgroundCheckButton = new System.Windows.Forms.Button();
            this.ExplicitUserRequestCheckButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkForUpdatesTimer
            // 
            this.checkForUpdatesTimer.Enabled = true;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = "hello";
            this.notifyIcon1.BalloonTipTitle = "hello";
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // AppBackgroundCheckButton
            // 
            this.AppBackgroundCheckButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AppBackgroundCheckButton.Location = new System.Drawing.Point(12, 37);
            this.AppBackgroundCheckButton.Name = "AppBackgroundCheckButton";
            this.AppBackgroundCheckButton.Size = new System.Drawing.Size(212, 23);
            this.AppBackgroundCheckButton.TabIndex = 0;
            this.AppBackgroundCheckButton.Text = "App Background Check";
            this.AppBackgroundCheckButton.UseVisualStyleBackColor = true;
            this.AppBackgroundCheckButton.Click += new System.EventHandler(this.AppBackgroundCheckButton_Click);
            // 
            // ExplicitUserRequestCheckButton
            // 
            this.ExplicitUserRequestCheckButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExplicitUserRequestCheckButton.Location = new System.Drawing.Point(12, 80);
            this.ExplicitUserRequestCheckButton.Name = "ExplicitUserRequestCheckButton";
            this.ExplicitUserRequestCheckButton.Size = new System.Drawing.Size(212, 23);
            this.ExplicitUserRequestCheckButton.TabIndex = 1;
            this.ExplicitUserRequestCheckButton.Text = "Explicit User Request To Check ";
            this.ExplicitUserRequestCheckButton.UseVisualStyleBackColor = true;
            this.ExplicitUserRequestCheckButton.Click += new System.EventHandler(this.ExplicitUserRequestCheckButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(248, 154);
            this.Controls.Add(this.ExplicitUserRequestCheckButton);
            this.Controls.Add(this.AppBackgroundCheckButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer checkForUpdatesTimer;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button AppBackgroundCheckButton;
        private System.Windows.Forms.Button ExplicitUserRequestCheckButton;
    }
}

