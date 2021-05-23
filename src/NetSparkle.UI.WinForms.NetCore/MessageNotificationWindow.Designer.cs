namespace NetSparkleUpdater.UI.WinForms
{
    partial class MessageNotificationWindow
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
            this.iconImage = new System.Windows.Forms.PictureBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.iconImage)).BeginInit();
            this.SuspendLayout();
            // 
            // iconImage
            // 
            this.iconImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.iconImage.Location = new System.Drawing.Point(16, 37);
            this.iconImage.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.iconImage.Name = "iconImage";
            this.iconImage.Size = new System.Drawing.Size(48, 48);
            this.iconImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconImage.TabIndex = 9;
            this.iconImage.TabStop = false;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblMessage.Location = new System.Drawing.Point(102, 37);
            this.lblMessage.MaximumSize = new System.Drawing.Size(682, 166);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(661, 126);
            this.lblMessage.TabIndex = 8;
            this.lblMessage.Text = "Sorry, either our server is having a problem, or your internet connection is inva" +
    "lid.";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(349, 246);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(140, 59);
            this.button1.TabIndex = 10;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OK_Click);
            // 
            // MessageNotificationWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 371);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.iconImage);
            this.Controls.Add(this.lblMessage);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageNotificationWindow";
            this.Text = "Message";
            ((System.ComponentModel.ISupportInitialize)(this.iconImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox iconImage;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button button1;
    }
}