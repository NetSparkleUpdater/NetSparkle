namespace NetSparkleChecker
{
    partial class NetSparkleCheckerWaitUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetSparkleCheckerWaitUI));
            this.lblHeader = new System.Windows.Forms.Label();
            this.imgAppIcon = new System.Windows.Forms.PictureBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.bckWorker = new System.ComponentModel.BackgroundWorker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblRefFileName = new System.Windows.Forms.Label();
            this.lblRefUrl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(66, 12);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(184, 17);
            this.lblHeader.TabIndex = 7;
            this.lblHeader.Text = "Checking for new updates....";
            // 
            // imgAppIcon
            // 
            this.imgAppIcon.Image = global::NetSparkleChecker.Properties.Resources.software_update_available;
            this.imgAppIcon.Location = new System.Drawing.Point(12, 12);
            this.imgAppIcon.Name = "imgAppIcon";
            this.imgAppIcon.Size = new System.Drawing.Size(48, 48);
            this.imgAppIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgAppIcon.TabIndex = 6;
            this.imgAppIcon.TabStop = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(69, 37);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(402, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 8;
            // 
            // bckWorker
            // 
            this.bckWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bckWorker_DoWork);
            this.bckWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bckWorker_RunWorkerCompleted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "References file:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "App-Cast:";
            // 
            // lblRefFileName
            // 
            this.lblRefFileName.AutoSize = true;
            this.lblRefFileName.Location = new System.Drawing.Point(99, 73);
            this.lblRefFileName.Name = "lblRefFileName";
            this.lblRefFileName.Size = new System.Drawing.Size(22, 13);
            this.lblRefFileName.TabIndex = 11;
            this.lblRefFileName.Text = "xxx";
            // 
            // lblRefUrl
            // 
            this.lblRefUrl.AutoSize = true;
            this.lblRefUrl.Location = new System.Drawing.Point(99, 95);
            this.lblRefUrl.Name = "lblRefUrl";
            this.lblRefUrl.Size = new System.Drawing.Size(22, 13);
            this.lblRefUrl.TabIndex = 12;
            this.lblRefUrl.Text = "xxx";
            // 
            // NetSparkleCheckerWaitUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(483, 118);
            this.Controls.Add(this.lblRefUrl);
            this.Controls.Add(this.lblRefFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.imgAppIcon);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NetSparkleCheckerWaitUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Checking for new updates";
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.PictureBox imgAppIcon;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.ComponentModel.BackgroundWorker bckWorker;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblRefFileName;
        private System.Windows.Forms.Label lblRefUrl;
    }
}

