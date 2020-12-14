namespace AzLog
{
    partial class AzAddDatasource_TextFile
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
            this.button1 = new System.Windows.Forms.Button();
            this.m_pbOpen = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.m_ebName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(652, 393);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(135, 35);
            this.button1.TabIndex = 14;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // m_pbOpen
            // 
            this.m_pbOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbOpen.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_pbOpen.Location = new System.Drawing.Point(652, 348);
            this.m_pbOpen.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_pbOpen.Name = "m_pbOpen";
            this.m_pbOpen.Size = new System.Drawing.Size(135, 35);
            this.m_pbOpen.TabIndex = 13;
            this.m_pbOpen.Text = "OK";
            this.m_pbOpen.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 20);
            this.label3.TabIndex = 15;
            this.label3.Text = "Log Filename";
            // 
            // m_ebName
            // 
            this.m_ebName.Location = new System.Drawing.Point(126, 6);
            this.m_ebName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_ebName.Name = "m_ebName";
            this.m_ebName.Size = new System.Drawing.Size(180, 26);
            this.m_ebName.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(314, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 20);
            this.label1.TabIndex = 17;
            this.label1.Text = "(blank == prompt)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 42);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 20);
            this.label2.TabIndex = 18;
            this.label2.Text = "Line Format";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(126, 39);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(661, 26);
            this.textBox1.TabIndex = 19;
            // 
            // AzAddDatasource_TextFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_ebName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.m_pbOpen);
            this.Name = "AzAddDatasource_TextFile";
            this.Text = "AzAddDatasource_TextFile";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button m_pbOpen;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_ebName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
    }
}