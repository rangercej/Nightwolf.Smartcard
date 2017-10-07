namespace Nightwolf.SmartTrigger
{
    partial class ScPinWindow
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
            this.textPin = new System.Windows.Forms.TextBox();
            this.btnUnlock = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Enter PIN";
            // 
            // textPin
            // 
            this.textPin.Location = new System.Drawing.Point(71, 12);
            this.textPin.Name = "textPin";
            this.textPin.Size = new System.Drawing.Size(129, 20);
            this.textPin.TabIndex = 1;
            // 
            // btnUnlock
            // 
            this.btnUnlock.Location = new System.Drawing.Point(206, 12);
            this.btnUnlock.Name = "btnUnlock";
            this.btnUnlock.Size = new System.Drawing.Size(75, 19);
            this.btnUnlock.TabIndex = 2;
            this.btnUnlock.Text = "Unlock";
            this.btnUnlock.UseVisualStyleBackColor = true;
            this.btnUnlock.Click += new System.EventHandler(this.btnUnlock_Click);
            // 
            // ScPinWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 60);
            this.Controls.Add(this.btnUnlock);
            this.Controls.Add(this.textPin);
            this.Controls.Add(this.label1);
            this.Name = "ScPinWindow";
            this.Text = "Smartcard PIN";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScPinWindow_FormClosing);
            this.Resize += new System.EventHandler(this.ScPinWindow_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textPin;
        private System.Windows.Forms.Button btnUnlock;
    }
}

