namespace NetSparkle.UI.WinForms
{
	partial class ToastNotifier
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
            this._message = new System.Windows.Forms.Label();
            this.Image = new System.Windows.Forms.PictureBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this._callToAction = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.Image)).BeginInit();
            this.SuspendLayout();
            // 
            // _message
            // 
            this._message.AutoSize = true;
            this._message.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._message.Location = new System.Drawing.Point(142, 17);
            this._message.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this._message.MaximumSize = new System.Drawing.Size(380, 0);
            this._message.Name = "_message";
            this._message.Size = new System.Drawing.Size(247, 37);
            this._message.TabIndex = 2;
            this._message.Text = "Notification Text";
            this._message.Click += new System.EventHandler(this.ToastNotifier_Click);
            // 
            // Image
            // 
            this.Image.Dock = System.Windows.Forms.DockStyle.Left;
            this.Image.Location = new System.Drawing.Point(0, 0);
            this.Image.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Image.Name = "Image";
            this.Image.Size = new System.Drawing.Size(128, 115);
            this.Image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Image.TabIndex = 3;
            this.Image.TabStop = false;
            this.Image.Click += new System.EventHandler(this.ToastNotifier_Click);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _callToAction
            // 
            this._callToAction.AutoSize = true;
            this._callToAction.Location = new System.Drawing.Point(144, 75);
            this._callToAction.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this._callToAction.Name = "_callToAction";
            this._callToAction.Size = new System.Drawing.Size(116, 25);
            this._callToAction.TabIndex = 4;
            this._callToAction.TabStop = true;
            this._callToAction.Text = "link Label1";
            this._callToAction.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.callToAction_LinkClicked);
            this._callToAction.Click += new System.EventHandler(this.ToastNotifier_Click);
            // 
            // ToastNotifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(480, 115);
            this.ControlBox = false;
            this.Controls.Add(this._callToAction);
            this.Controls.Add(this.Image);
            this.Controls.Add(this._message);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ToastNotifier";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Click += new System.EventHandler(this.ToastNotifier_Click);
            ((System.ComponentModel.ISupportInitialize)(this.Image)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Label _message;
        private System.Windows.Forms.ImageList imageList1;
        
        /// <summary>
        /// Image of your app
        /// </summary>
        public System.Windows.Forms.PictureBox Image;
        private System.Windows.Forms.LinkLabel _callToAction;
	}
}