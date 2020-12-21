namespace AzLog
{
    partial class AzEditFilters
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
            this.m_lbFilters = new System.Windows.Forms.ListBox();
            this.m_pbDelete = new System.Windows.Forms.Button();
            this.m_pbOK = new System.Windows.Forms.Button();
            this.m_pbCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_lbFilters
            // 
            this.m_lbFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lbFilters.FormattingEnabled = true;
            this.m_lbFilters.Location = new System.Drawing.Point(12, 12);
            this.m_lbFilters.Name = "m_lbFilters";
            this.m_lbFilters.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.m_lbFilters.Size = new System.Drawing.Size(522, 186);
            this.m_lbFilters.TabIndex = 0;
            // 
            // m_pbDelete
            // 
            this.m_pbDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_pbDelete.Location = new System.Drawing.Point(12, 210);
            this.m_pbDelete.Name = "m_pbDelete";
            this.m_pbDelete.Size = new System.Drawing.Size(75, 23);
            this.m_pbDelete.TabIndex = 1;
            this.m_pbDelete.Text = "Delete";
            this.m_pbDelete.UseVisualStyleBackColor = true;
            this.m_pbDelete.Click += new System.EventHandler(this.DoDeleteItem);
            // 
            // m_pbOK
            // 
            this.m_pbOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_pbOK.Location = new System.Drawing.Point(378, 210);
            this.m_pbOK.Name = "m_pbOK";
            this.m_pbOK.Size = new System.Drawing.Size(75, 23);
            this.m_pbOK.TabIndex = 2;
            this.m_pbOK.Text = "OK";
            this.m_pbOK.UseVisualStyleBackColor = true;
            // 
            // m_pbCancel
            // 
            this.m_pbCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_pbCancel.Location = new System.Drawing.Point(459, 210);
            this.m_pbCancel.Name = "m_pbCancel";
            this.m_pbCancel.Size = new System.Drawing.Size(75, 23);
            this.m_pbCancel.TabIndex = 3;
            this.m_pbCancel.Text = "Cancel";
            this.m_pbCancel.UseVisualStyleBackColor = true;
            // 
            // AzEditFilters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 241);
            this.Controls.Add(this.m_pbCancel);
            this.Controls.Add(this.m_pbOK);
            this.Controls.Add(this.m_pbDelete);
            this.Controls.Add(this.m_lbFilters);
            this.Name = "AzEditFilters";
            this.Text = "AzEditFilters";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox m_lbFilters;
        private System.Windows.Forms.Button m_pbDelete;
        private System.Windows.Forms.Button m_pbOK;
        private System.Windows.Forms.Button m_pbCancel;
    }
}