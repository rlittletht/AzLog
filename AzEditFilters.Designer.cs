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
			System.Windows.Forms.Label label1;
			this.m_pbDelete = new System.Windows.Forms.Button();
			this.m_pbOK = new System.Windows.Forms.Button();
			this.m_pbCancel = new System.Windows.Forms.Button();
			this.m_ebFilter = new System.Windows.Forms.TextBox();
			label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(13, 13);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(84, 20);
			label1.TabIndex = 4;
			label1.Text = "Item Filter:";
			// 
			// m_pbDelete
			// 
			this.m_pbDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_pbDelete.Location = new System.Drawing.Point(18, 323);
			this.m_pbDelete.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbDelete.Name = "m_pbDelete";
			this.m_pbDelete.Size = new System.Drawing.Size(112, 35);
			this.m_pbDelete.TabIndex = 1;
			this.m_pbDelete.Text = "Delete";
			this.m_pbDelete.UseVisualStyleBackColor = true;
			// 
			// m_pbOK
			// 
			this.m_pbOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_pbOK.Location = new System.Drawing.Point(567, 323);
			this.m_pbOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbOK.Name = "m_pbOK";
			this.m_pbOK.Size = new System.Drawing.Size(112, 35);
			this.m_pbOK.TabIndex = 2;
			this.m_pbOK.Text = "OK";
			this.m_pbOK.UseVisualStyleBackColor = true;
			// 
			// m_pbCancel
			// 
			this.m_pbCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_pbCancel.Location = new System.Drawing.Point(688, 323);
			this.m_pbCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbCancel.Name = "m_pbCancel";
			this.m_pbCancel.Size = new System.Drawing.Size(112, 35);
			this.m_pbCancel.TabIndex = 3;
			this.m_pbCancel.Text = "Cancel";
			this.m_pbCancel.UseVisualStyleBackColor = true;
			// 
			// m_ebFilter
			// 
			this.m_ebFilter.AcceptsReturn = true;
			this.m_ebFilter.Location = new System.Drawing.Point(18, 37);
			this.m_ebFilter.Multiline = true;
			this.m_ebFilter.Name = "m_ebFilter";
			this.m_ebFilter.Size = new System.Drawing.Size(782, 278);
			this.m_ebFilter.TabIndex = 5;
			// 
			// AzEditFilters
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(820, 371);
			this.Controls.Add(this.m_ebFilter);
			this.Controls.Add(label1);
			this.Controls.Add(this.m_pbCancel);
			this.Controls.Add(this.m_pbOK);
			this.Controls.Add(this.m_pbDelete);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "AzEditFilters";
			this.Text = "AzEditFilters";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button m_pbDelete;
        private System.Windows.Forms.Button m_pbOK;
        private System.Windows.Forms.Button m_pbCancel;
		private System.Windows.Forms.TextBox m_ebFilter;
	}
}