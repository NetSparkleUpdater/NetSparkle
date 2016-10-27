namespace NetSparkleTestApp
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnTestLoop = new System.Windows.Forms.Button();
            this.btnStopLoop = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(92, 46);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(347, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Close this dialog to exit NetSparkle";
            // 
            // btnTestLoop
            // 
            this.btnTestLoop.Location = new System.Drawing.Point(24, 113);
            this.btnTestLoop.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnTestLoop.Name = "btnTestLoop";
            this.btnTestLoop.Size = new System.Drawing.Size(150, 44);
            this.btnTestLoop.TabIndex = 1;
            this.btnTestLoop.Text = "Test Loop";
            this.btnTestLoop.UseVisualStyleBackColor = true;
            this.btnTestLoop.Click += new System.EventHandler(this.btnTestLoop_Click);
            // 
            // btnStopLoop
            // 
            this.btnStopLoop.Location = new System.Drawing.Point(186, 113);
            this.btnStopLoop.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnStopLoop.Name = "btnStopLoop";
            this.btnStopLoop.Size = new System.Drawing.Size(150, 44);
            this.btnStopLoop.TabIndex = 2;
            this.btnStopLoop.Text = "Stop Loop";
            this.btnStopLoop.UseVisualStyleBackColor = true;
            this.btnStopLoop.Click += new System.EventHandler(this.btnStopLoop_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.Location = new System.Drawing.Point(348, 113);
            this.btnCheck.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(150, 44);
            this.btnCheck.TabIndex = 3;
            this.btnCheck.Text = "Check";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 82);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(468, 25);
            this.label2.TabIndex = 4;
            this.label2.Text = "NOTE: This demo is not working at the moment.";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 181);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCheck);
            this.Controls.Add(this.btnStopLoop);
            this.Controls.Add(this.btnTestLoop);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Location = new System.Drawing.Point(10, 10);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnTestLoop;
        private System.Windows.Forms.Button btnStopLoop;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Label label2;
    }
}

