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
            this.components = new System.ComponentModel.Container();
            this.m_pbFetch = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.m_ebEnd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.m_ebStart = new System.Windows.Forms.TextBox();
            this.m_lvLog = new System.Windows.Forms.ListView();
            this.m_ctxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.m_tsiShowHide = new System.Windows.Forms.ToolStripMenuItem();
            this.m_cbView = new System.Windows.Forms.ComboBox();
            this.m_pbSave = new System.Windows.Forms.Button();
            this.ctxMenuHeader = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.blahToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_ctxMenu.SuspendLayout();
            this.ctxMenuHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_pbFetch
            // 
            this.m_pbFetch.Location = new System.Drawing.Point(811, 57);
            this.m_pbFetch.Name = "m_pbFetch";
            this.m_pbFetch.Size = new System.Drawing.Size(75, 23);
            this.m_pbFetch.TabIndex = 11;
            this.m_pbFetch.Text = "Fetch";
            this.m_pbFetch.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(187, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "End";
            // 
            // m_ebEnd
            // 
            this.m_ebEnd.Location = new System.Drawing.Point(219, 47);
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
            this.m_ebStart.Location = new System.Drawing.Point(81, 47);
            this.m_ebStart.Name = "m_ebStart";
            this.m_ebStart.Size = new System.Drawing.Size(100, 20);
            this.m_ebStart.TabIndex = 14;
            this.m_ebStart.Text = "10/26/2015 8:00";
            // 
            // m_lvLog
            // 
            this.m_lvLog.AllowColumnReorder = true;
            this.m_lvLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lvLog.ContextMenuStrip = this.m_ctxMenu;
            this.m_lvLog.Location = new System.Drawing.Point(12, 86);
            this.m_lvLog.Name = "m_lvLog";
            this.m_lvLog.Size = new System.Drawing.Size(874, 525);
            this.m_lvLog.TabIndex = 16;
            this.m_lvLog.UseCompatibleStateImageBehavior = false;
            this.m_lvLog.View = System.Windows.Forms.View.Details;
            this.m_lvLog.VirtualMode = true;
            this.m_lvLog.ColumnReordered += new System.Windows.Forms.ColumnReorderedEventHandler(this.DoColumnReorder);
            this.m_lvLog.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.GetListViewItem);
            // 
            // m_ctxMenu
            // 
            this.m_ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsiShowHide});
            this.m_ctxMenu.Name = "m_ctxMenu";
            this.m_ctxMenu.Size = new System.Drawing.Size(146, 26);
            this.m_ctxMenu.Opening += new System.ComponentModel.CancelEventHandler(this.HandleContextOpening);
            // 
            // m_tsiShowHide
            // 
            this.m_tsiShowHide.Name = "m_tsiShowHide";
            this.m_tsiShowHide.Size = new System.Drawing.Size(145, 22);
            this.m_tsiShowHide.Text = "Hide Column";
            this.m_tsiShowHide.Click += new System.EventHandler(this.DoHideColumnClick);
            // 
            // m_cbView
            // 
            this.m_cbView.FormattingEnabled = true;
            this.m_cbView.Location = new System.Drawing.Point(348, 46);
            this.m_cbView.Name = "m_cbView";
            this.m_cbView.Size = new System.Drawing.Size(121, 21);
            this.m_cbView.TabIndex = 17;
            this.m_cbView.SelectedIndexChanged += new System.EventHandler(this.ChangeViewSelected);
            // 
            // m_pbSave
            // 
            this.m_pbSave.Location = new System.Drawing.Point(485, 45);
            this.m_pbSave.Name = "m_pbSave";
            this.m_pbSave.Size = new System.Drawing.Size(75, 23);
            this.m_pbSave.TabIndex = 18;
            this.m_pbSave.Text = "Save View";
            this.m_pbSave.UseVisualStyleBackColor = true;
            this.m_pbSave.Click += new System.EventHandler(this.DoViewSave);
            // 
            // ctxMenuHeader
            // 
            this.ctxMenuHeader.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.blahToolStripMenuItem});
            this.ctxMenuHeader.Name = "ctxMenuHeader";
            this.ctxMenuHeader.Size = new System.Drawing.Size(153, 48);
            // 
            // blahToolStripMenuItem
            // 
            this.blahToolStripMenuItem.Name = "blahToolStripMenuItem";
            this.blahToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.blahToolStripMenuItem.Text = "blah";
            this.blahToolStripMenuItem.Click += new System.EventHandler(this.blahToolStripMenuItem_Click);
            // 
            // AzLogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 623);
            this.Controls.Add(this.m_pbSave);
            this.Controls.Add(this.m_cbView);
            this.Controls.Add(this.m_lvLog);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_ebStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_ebEnd);
            this.Controls.Add(this.m_pbFetch);
            this.Name = "AzLogWindow";
            this.Text = "AzLogWindow";
            this.m_ctxMenu.ResumeLayout(false);
            this.ctxMenuHeader.ResumeLayout(false);
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
        private System.Windows.Forms.ComboBox m_cbView;
        private System.Windows.Forms.Button m_pbSave;
        private System.Windows.Forms.ContextMenuStrip m_ctxMenu;
        private System.Windows.Forms.ToolStripMenuItem m_tsiShowHide;
        private System.Windows.Forms.ContextMenuStrip ctxMenuHeader;
        private System.Windows.Forms.ToolStripMenuItem blahToolStripMenuItem;
    }
}