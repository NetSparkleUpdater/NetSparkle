namespace NetSparkleChecker
{
    partial class SoftwareUpToDate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SoftwareUpToDate));
            this.lblHeader = new System.Windows.Forms.Label();
            this.imgAppIcon = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(66, 12);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(156, 17);
            this.lblHeader.TabIndex = 9;
            this.lblHeader.Text = "Software is up to date...";
            // 
            // imgAppIcon
            // 
            this.imgAppIcon.Image = global::NetSparkleChecker.Properties.Resources.software_update_available;
            this.imgAppIcon.Location = new System.Drawing.Point(12, 12);
            this.imgAppIcon.Name = "imgAppIcon";
            this.imgAppIcon.Size = new System.Drawing.Size(48, 48);
            this.imgAppIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgAppIcon.TabIndex = 8;
            this.imgAppIcon.TabStop = false;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(66, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(307, 31);
            this.label1.TabIndex = 10;
            this.label1.Text = "The update check did not found a new version on the server. The software is up to" +
                " date!";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(147, 67);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 11;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // SoftwareUpToDate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 102);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.imgAppIcon);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SoftwareUpToDate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Software up to date";
            ((System.ComponentModel.ISupportInitialize)(this.imgAppIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.PictureBox imgAppIcon;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOk;
    }
}