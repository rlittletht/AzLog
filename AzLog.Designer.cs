namespace AzLog
{
    partial class AzLog
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
            this.m_cbAccounts = new System.Windows.Forms.ComboBox();
            this.m_pbAddAccount = new System.Windows.Forms.Button();
            this.m_pbRemove = new System.Windows.Forms.Button();
            this.m_lbTables = new System.Windows.Forms.ListBox();
            this.m_pbOpen = new System.Windows.Forms.Button();
            this.m_ebStart = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_ebEnd = new System.Windows.Forms.TextBox();
            this.m_pbFetch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_cbAccounts
            // 
            this.m_cbAccounts.FormattingEnabled = true;
            this.m_cbAccounts.Location = new System.Drawing.Point(13, 13);
            this.m_cbAccounts.Name = "m_cbAccounts";
            this.m_cbAccounts.Size = new System.Drawing.Size(121, 21);
            this.m_cbAccounts.TabIndex = 0;
            // 
            // m_pbAddAccount
            // 
            this.m_pbAddAccount.Location = new System.Drawing.Point(141, 13);
            this.m_pbAddAccount.Name = "m_pbAddAccount";
            this.m_pbAddAccount.Size = new System.Drawing.Size(111, 23);
            this.m_pbAddAccount.TabIndex = 1;
            this.m_pbAddAccount.Text = "Add/Edit Account";
            this.m_pbAddAccount.UseVisualStyleBackColor = true;
            this.m_pbAddAccount.Click += new System.EventHandler(this.DoAddEditAccount);
            // 
            // m_pbRemove
            // 
            this.m_pbRemove.Location = new System.Drawing.Point(258, 13);
            this.m_pbRemove.Name = "m_pbRemove";
            this.m_pbRemove.Size = new System.Drawing.Size(90, 23);
            this.m_pbRemove.TabIndex = 2;
            this.m_pbRemove.Text = "Remove";
            this.m_pbRemove.UseVisualStyleBackColor = true;
            // 
            // m_lbTables
            // 
            this.m_lbTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_lbTables.FormattingEnabled = true;
            this.m_lbTables.Location = new System.Drawing.Point(13, 82);
            this.m_lbTables.Name = "m_lbTables";
            this.m_lbTables.Size = new System.Drawing.Size(113, 134);
            this.m_lbTables.TabIndex = 3;
            this.m_lbTables.SelectedIndexChanged += new System.EventHandler(this.DoSelectTable);
            // 
            // m_pbOpen
            // 
            this.m_pbOpen.Location = new System.Drawing.Point(354, 13);
            this.m_pbOpen.Name = "m_pbOpen";
            this.m_pbOpen.Size = new System.Drawing.Size(90, 23);
            this.m_pbOpen.TabIndex = 4;
            this.m_pbOpen.Text = "Open Account";
            this.m_pbOpen.UseVisualStyleBackColor = true;
            this.m_pbOpen.Click += new System.EventHandler(this.m_pbOpen_Click);
            // 
            // m_ebStart
            // 
            this.m_ebStart.Location = new System.Drawing.Point(258, 56);
            this.m_ebStart.Name = "m_ebStart";
            this.m_ebStart.Size = new System.Drawing.Size(100, 20);
            this.m_ebStart.TabIndex = 6;
            this.m_ebStart.Text = "10/26/2015 8:00";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(199, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Start";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(376, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "End";
            // 
            // m_ebEnd
            // 
            this.m_ebEnd.Location = new System.Drawing.Point(435, 56);
            this.m_ebEnd.Name = "m_ebEnd";
            this.m_ebEnd.Size = new System.Drawing.Size(100, 20);
            this.m_ebEnd.TabIndex = 8;
            this.m_ebEnd.Text = "10/26/2015 9:00";
            // 
            // m_pbFetch
            // 
            this.m_pbFetch.Location = new System.Drawing.Point(556, 53);
            this.m_pbFetch.Name = "m_pbFetch";
            this.m_pbFetch.Size = new System.Drawing.Size(75, 23);
            this.m_pbFetch.TabIndex = 10;
            this.m_pbFetch.Text = "Fetch";
            this.m_pbFetch.UseVisualStyleBackColor = true;
            this.m_pbFetch.Click += new System.EventHandler(this.DoFetchLogEntries);
            // 
            // AzLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 231);
            this.Controls.Add(this.m_pbFetch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_ebEnd);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_ebStart);
            this.Controls.Add(this.m_pbOpen);
            this.Controls.Add(this.m_lbTables);
            this.Controls.Add(this.m_pbRemove);
            this.Controls.Add(this.m_pbAddAccount);
            this.Controls.Add(this.m_cbAccounts);
            this.Name = "AzLog";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox m_cbAccounts;
        private System.Windows.Forms.Button m_pbAddAccount;
        private System.Windows.Forms.Button m_pbRemove;
        private System.Windows.Forms.ListBox m_lbTables;
        private System.Windows.Forms.Button m_pbOpen;
        private System.Windows.Forms.TextBox m_ebStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox m_ebEnd;
        private System.Windows.Forms.Button m_pbFetch;
    }
}

