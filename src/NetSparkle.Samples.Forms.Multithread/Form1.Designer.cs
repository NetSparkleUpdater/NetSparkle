namespace NetSparkle.Samples.Forms.Multithread
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
            // ExplicitUserRequestCheckButton
            // 
            this.ExplicitUserRequestCheckButton.Location = new System.Drawing.Point(12, 40);
            this.ExplicitUserRequestCheckButton.Name = "ExplicitUserRequestCheckButton";
            this.ExplicitUserRequestCheckButton.Size = new System.Drawing.Size(212, 23);
            this.ExplicitUserRequestCheckButton.TabIndex = 1;
            this.ExplicitUserRequestCheckButton.Text = "Check for Update ";
            this.ExplicitUserRequestCheckButton.UseVisualStyleBackColor = true;
            this.ExplicitUserRequestCheckButton.Click += new System.EventHandler(this.ExplicitUserRequestCheckButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 154);
            this.Controls.Add(this.ExplicitUserRequestCheckButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer checkForUpdatesTimer;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button ExplicitUserRequestCheckButton;
    }
}

