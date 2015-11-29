namespace AzLog
{
    partial class AzAddDatasource_Azure
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
            this.m_pbOpen = new System.Windows.Forms.Button();
            this.m_lbTables = new System.Windows.Forms.ListBox();
            this.m_pbRemove = new System.Windows.Forms.Button();
            this.m_pbAddAccount = new System.Windows.Forms.Button();
            this.m_cbAccounts = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.m_ebName = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_pbOpen
            // 
            this.m_pbOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbOpen.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_pbOpen.Location = new System.Drawing.Point(302, 288);
            this.m_pbOpen.Name = "m_pbOpen";
            this.m_pbOpen.Size = new System.Drawing.Size(90, 23);
            this.m_pbOpen.TabIndex = 9;
            this.m_pbOpen.Text = "OK";
            this.m_pbOpen.UseVisualStyleBackColor = true;
            // 
            // m_lbTables
            // 
            this.m_lbTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_lbTables.FormattingEnabled = true;
            this.m_lbTables.Location = new System.Drawing.Point(12, 115);
            this.m_lbTables.Name = "m_lbTables";
            this.m_lbTables.Size = new System.Drawing.Size(121, 225);
            this.m_lbTables.TabIndex = 8;
            this.m_lbTables.SelectedIndexChanged += new System.EventHandler(this.DoSelectTable);
            // 
            // m_pbRemove
            // 
            this.m_pbRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbRemove.Location = new System.Drawing.Point(281, 41);
            this.m_pbRemove.Name = "m_pbRemove";
            this.m_pbRemove.Size = new System.Drawing.Size(111, 23);
            this.m_pbRemove.TabIndex = 7;
            this.m_pbRemove.Text = "Remove Account";
            this.m_pbRemove.UseVisualStyleBackColor = true;
            // 
            // m_pbAddAccount
            // 
            this.m_pbAddAccount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbAddAccount.Location = new System.Drawing.Point(281, 12);
            this.m_pbAddAccount.Name = "m_pbAddAccount";
            this.m_pbAddAccount.Size = new System.Drawing.Size(111, 23);
            this.m_pbAddAccount.TabIndex = 6;
            this.m_pbAddAccount.Text = "Add/Edit Account";
            this.m_pbAddAccount.UseVisualStyleBackColor = true;
            this.m_pbAddAccount.Click += new System.EventHandler(this.DoAddEditAccount);
            // 
            // m_cbAccounts
            // 
            this.m_cbAccounts.FormattingEnabled = true;
            this.m_cbAccounts.Location = new System.Drawing.Point(132, 46);
            this.m_cbAccounts.Name = "m_cbAccounts";
            this.m_cbAccounts.Size = new System.Drawing.Size(121, 21);
            this.m_cbAccounts.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Azure storage account:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Choose storage table";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(302, 317);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 23);
            this.button1.TabIndex = 12;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Datasource Name:";
            // 
            // m_ebName
            // 
            this.m_ebName.Location = new System.Drawing.Point(132, 19);
            this.m_ebName.Name = "m_ebName";
            this.m_ebName.Size = new System.Drawing.Size(121, 20);
            this.m_ebName.TabIndex = 14;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(281, 115);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(111, 23);
            this.button2.TabIndex = 15;
            this.button2.Text = "Refresh Tables";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.DoOpenAccount);
            // 
            // AzAddDatasource_Azure
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 356);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.m_ebName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_pbOpen);
            this.Controls.Add(this.m_lbTables);
            this.Controls.Add(this.m_pbRemove);
            this.Controls.Add(this.m_pbAddAccount);
            this.Controls.Add(this.m_cbAccounts);
            this.Name = "AzAddDatasource_Azure";
            this.Text = "AzAddDatasource_Azure";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_pbOpen;
        private System.Windows.Forms.ListBox m_lbTables;
        private System.Windows.Forms.Button m_pbRemove;
        private System.Windows.Forms.Button m_pbAddAccount;
        private System.Windows.Forms.ComboBox m_cbAccounts;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_ebName;
        private System.Windows.Forms.Button button2;
    }
}