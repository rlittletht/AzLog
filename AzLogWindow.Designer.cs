namespace AzLog
{
    partial class AzLogWindow
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
            this.m_pbFetch = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.m_ebEnd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.m_ebStart = new System.Windows.Forms.TextBox();
            this.m_lvLog = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // m_pbFetch
            // 
            this.m_pbFetch.Location = new System.Drawing.Point(447, 40);
            this.m_pbFetch.Name = "m_pbFetch";
            this.m_pbFetch.Size = new System.Drawing.Size(75, 23);
            this.m_pbFetch.TabIndex = 11;
            this.m_pbFetch.Text = "Fetch";
            this.m_pbFetch.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(211, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "End";
            // 
            // m_ebEnd
            // 
            this.m_ebEnd.Location = new System.Drawing.Point(270, 50);
            this.m_ebEnd.Name = "m_ebEnd";
            this.m_ebEnd.Size = new System.Drawing.Size(100, 20);
            this.m_ebEnd.TabIndex = 12;
            this.m_ebEnd.Text = "10/26/2015 9:00";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(46, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Start";
            // 
            // m_ebStart
            // 
            this.m_ebStart.Location = new System.Drawing.Point(105, 50);
            this.m_ebStart.Name = "m_ebStart";
            this.m_ebStart.Size = new System.Drawing.Size(100, 20);
            this.m_ebStart.TabIndex = 14;
            this.m_ebStart.Text = "10/26/2015 8:00";
            // 
            // m_lvLog
            // 
            this.m_lvLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lvLog.Location = new System.Drawing.Point(12, 86);
            this.m_lvLog.Name = "m_lvLog";
            this.m_lvLog.Size = new System.Drawing.Size(541, 375);
            this.m_lvLog.TabIndex = 16;
            this.m_lvLog.UseCompatibleStateImageBehavior = false;
            this.m_lvLog.View = System.Windows.Forms.View.Details;
            this.m_lvLog.VirtualMode = true;
            this.m_lvLog.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.GetListViewItem);
            // 
            // AzLogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 473);
            this.Controls.Add(this.m_lvLog);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_ebStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_ebEnd);
            this.Controls.Add(this.m_pbFetch);
            this.Name = "AzLogWindow";
            this.Text = "AzLogWindow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_pbFetch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox m_ebEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox m_ebStart;
        private System.Windows.Forms.ListView m_lvLog;
    }
}