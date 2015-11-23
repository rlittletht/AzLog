namespace AzLog
{
    partial class AzAddAccount
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            this.m_ebAccountName = new System.Windows.Forms.TextBox();
            this.m_ebAccountKey = new System.Windows.Forms.TextBox();
            this.m_cbStorageType = new System.Windows.Forms.ComboBox();
            this.m_ebAccountDomain = new System.Windows.Forms.TextBox();
            this.m_pbTest = new System.Windows.Forms.Button();
            this.m_pbSave = new System.Windows.Forms.Button();
            this.m_pbCancel = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(90, 13);
            label1.TabIndex = 0;
            label1.Text = "Storage Account:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 39);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(68, 13);
            label2.TabIndex = 2;
            label2.Text = "Account Key";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 65);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(43, 13);
            label3.TabIndex = 5;
            label3.Text = "Domain";
            // 
            // m_ebAccountName
            // 
            this.m_ebAccountName.Location = new System.Drawing.Point(109, 10);
            this.m_ebAccountName.Name = "m_ebAccountName";
            this.m_ebAccountName.Size = new System.Drawing.Size(284, 20);
            this.m_ebAccountName.TabIndex = 1;
            // 
            // m_ebAccountKey
            // 
            this.m_ebAccountKey.Location = new System.Drawing.Point(109, 36);
            this.m_ebAccountKey.Name = "m_ebAccountKey";
            this.m_ebAccountKey.Size = new System.Drawing.Size(284, 20);
            this.m_ebAccountKey.TabIndex = 3;
            // 
            // m_cbStorageType
            // 
            this.m_cbStorageType.FormattingEnabled = true;
            this.m_cbStorageType.Items.AddRange(new object[] {
            "Cloud Storage",
            "Developer Storage"});
            this.m_cbStorageType.Location = new System.Drawing.Point(399, 9);
            this.m_cbStorageType.Name = "m_cbStorageType";
            this.m_cbStorageType.Size = new System.Drawing.Size(121, 21);
            this.m_cbStorageType.TabIndex = 4;
            // 
            // m_ebAccountDomain
            // 
            this.m_ebAccountDomain.Location = new System.Drawing.Point(109, 62);
            this.m_ebAccountDomain.Name = "m_ebAccountDomain";
            this.m_ebAccountDomain.Size = new System.Drawing.Size(284, 20);
            this.m_ebAccountDomain.TabIndex = 6;
            this.m_ebAccountDomain.Text = "core.windows.net";
            // 
            // m_pbTest
            // 
            this.m_pbTest.Location = new System.Drawing.Point(283, 95);
            this.m_pbTest.Name = "m_pbTest";
            this.m_pbTest.Size = new System.Drawing.Size(75, 23);
            this.m_pbTest.TabIndex = 7;
            this.m_pbTest.Text = "Test";
            this.m_pbTest.UseVisualStyleBackColor = true;
            this.m_pbTest.Click += new System.EventHandler(this.DoTest);
            // 
            // m_pbSave
            // 
            this.m_pbSave.Location = new System.Drawing.Point(364, 95);
            this.m_pbSave.Name = "m_pbSave";
            this.m_pbSave.Size = new System.Drawing.Size(75, 23);
            this.m_pbSave.TabIndex = 8;
            this.m_pbSave.Text = "Save";
            this.m_pbSave.UseVisualStyleBackColor = true;
            this.m_pbSave.Click += new System.EventHandler(this.DoSave);
            // 
            // m_pbCancel
            // 
            this.m_pbCancel.Location = new System.Drawing.Point(445, 95);
            this.m_pbCancel.Name = "m_pbCancel";
            this.m_pbCancel.Size = new System.Drawing.Size(75, 23);
            this.m_pbCancel.TabIndex = 9;
            this.m_pbCancel.Text = "Cancel";
            this.m_pbCancel.UseVisualStyleBackColor = true;
            this.m_pbCancel.Click += new System.EventHandler(this.DoCancel);
            // 
            // AzAddAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 127);
            this.Controls.Add(this.m_pbCancel);
            this.Controls.Add(this.m_pbSave);
            this.Controls.Add(this.m_pbTest);
            this.Controls.Add(this.m_ebAccountDomain);
            this.Controls.Add(label3);
            this.Controls.Add(this.m_cbStorageType);
            this.Controls.Add(this.m_ebAccountKey);
            this.Controls.Add(label2);
            this.Controls.Add(this.m_ebAccountName);
            this.Controls.Add(label1);
            this.Name = "AzAddAccount";
            this.Text = "AzAddAccount";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox m_ebAccountName;
        private System.Windows.Forms.TextBox m_ebAccountKey;
        private System.Windows.Forms.ComboBox m_cbStorageType;
        private System.Windows.Forms.TextBox m_ebAccountDomain;
        private System.Windows.Forms.Button m_pbTest;
        private System.Windows.Forms.Button m_pbSave;
        private System.Windows.Forms.Button m_pbCancel;
    }
}