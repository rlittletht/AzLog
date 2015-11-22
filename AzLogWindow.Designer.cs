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
            this.ctxMenuList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.m_cbView = new System.Windows.Forms.ComboBox();
            this.m_pbSave = new System.Windows.Forms.Button();
            this.m_ctxmHeader = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.m_pbEndF = new System.Windows.Forms.Button();
            this.m_pbEndFF = new System.Windows.Forms.Button();
            this.m_pbStartFF = new System.Windows.Forms.Button();
            this.m_pbStartF = new System.Windows.Forms.Button();
            this.m_pbEndR = new System.Windows.Forms.Button();
            this.m_pbEndFR = new System.Windows.Forms.Button();
            this.m_pbStartR = new System.Windows.Forms.Button();
            this.m_pbStartFR = new System.Windows.Forms.Button();
            this.ctxMenuList.SuspendLayout();
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
            this.label2.Location = new System.Drawing.Point(326, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "End";
            // 
            // m_ebEnd
            // 
            this.m_ebEnd.Location = new System.Drawing.Point(289, 44);
            this.m_ebEnd.Name = "m_ebEnd";
            this.m_ebEnd.Size = new System.Drawing.Size(100, 20);
            this.m_ebEnd.TabIndex = 12;
            this.m_ebEnd.Text = "10/26/2015 9:00";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(78, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Start";
            // 
            // m_ebStart
            // 
            this.m_ebStart.Location = new System.Drawing.Point(63, 44);
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
            this.m_lvLog.ContextMenuStrip = this.ctxMenuList;
            this.m_lvLog.Location = new System.Drawing.Point(12, 86);
            this.m_lvLog.Name = "m_lvLog";
            this.m_lvLog.Size = new System.Drawing.Size(874, 525);
            this.m_lvLog.TabIndex = 16;
            this.m_lvLog.UseCompatibleStateImageBehavior = false;
            this.m_lvLog.View = System.Windows.Forms.View.Details;
            this.m_lvLog.VirtualMode = true;
            this.m_lvLog.ColumnReordered += new System.Windows.Forms.ColumnReorderedEventHandler(this.DoColumnReorder);
            this.m_lvLog.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.NotifyColumnWidthChanged);
            this.m_lvLog.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.GetListViewItem);
            // 
            // ctxMenuList
            // 
            this.ctxMenuList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFilter});
            this.ctxMenuList.Name = "ctxMenuList";
            this.ctxMenuList.Size = new System.Drawing.Size(146, 26);
            this.ctxMenuList.Opening += new System.ComponentModel.CancelEventHandler(this.HandleContextOpening);
            // 
            // tsmiFilter
            // 
            this.tsmiFilter.Name = "tsmiFilter";
            this.tsmiFilter.Size = new System.Drawing.Size(145, 22);
            this.tsmiFilter.Text = "Filter to this...";
            // 
            // m_cbView
            // 
            this.m_cbView.FormattingEnabled = true;
            this.m_cbView.Location = new System.Drawing.Point(580, 42);
            this.m_cbView.Name = "m_cbView";
            this.m_cbView.Size = new System.Drawing.Size(121, 21);
            this.m_cbView.TabIndex = 17;
            this.m_cbView.SelectedIndexChanged += new System.EventHandler(this.ChangeViewSelected);
            // 
            // m_pbSave
            // 
            this.m_pbSave.Location = new System.Drawing.Point(717, 41);
            this.m_pbSave.Name = "m_pbSave";
            this.m_pbSave.Size = new System.Drawing.Size(75, 23);
            this.m_pbSave.TabIndex = 18;
            this.m_pbSave.Text = "Save View";
            this.m_pbSave.UseVisualStyleBackColor = true;
            this.m_pbSave.Click += new System.EventHandler(this.DoViewSave);
            // 
            // m_ctxmHeader
            // 
            this.m_ctxmHeader.Name = "m_ctxmHeader";
            this.m_ctxmHeader.Size = new System.Drawing.Size(61, 4);
            // 
            // m_pbEndF
            // 
            this.m_pbEndF.Location = new System.Drawing.Point(384, 42);
            this.m_pbEndF.Name = "m_pbEndF";
            this.m_pbEndF.Size = new System.Drawing.Size(28, 23);
            this.m_pbEndF.TabIndex = 19;
            this.m_pbEndF.Text = ">";
            this.m_pbEndF.UseVisualStyleBackColor = true;
            this.m_pbEndF.Click += new System.EventHandler(this.EndBumpForward);
            // 
            // m_pbEndFF
            // 
            this.m_pbEndFF.Location = new System.Drawing.Point(409, 42);
            this.m_pbEndFF.Name = "m_pbEndFF";
            this.m_pbEndFF.Size = new System.Drawing.Size(28, 23);
            this.m_pbEndFF.TabIndex = 20;
            this.m_pbEndFF.Text = ">>";
            this.m_pbEndFF.UseVisualStyleBackColor = true;
            this.m_pbEndFF.Click += new System.EventHandler(this.EndBumpFastForward);
            // 
            // m_pbStartFF
            // 
            this.m_pbStartFF.Location = new System.Drawing.Point(185, 42);
            this.m_pbStartFF.Name = "m_pbStartFF";
            this.m_pbStartFF.Size = new System.Drawing.Size(28, 23);
            this.m_pbStartFF.TabIndex = 22;
            this.m_pbStartFF.Text = ">>";
            this.m_pbStartFF.UseVisualStyleBackColor = true;
            this.m_pbStartFF.Click += new System.EventHandler(this.StartBumpFastForward);
            // 
            // m_pbStartF
            // 
            this.m_pbStartF.Location = new System.Drawing.Point(160, 42);
            this.m_pbStartF.Name = "m_pbStartF";
            this.m_pbStartF.Size = new System.Drawing.Size(28, 23);
            this.m_pbStartF.TabIndex = 21;
            this.m_pbStartF.Text = ">";
            this.m_pbStartF.UseVisualStyleBackColor = true;
            this.m_pbStartF.Click += new System.EventHandler(this.StartBumpForward);
            // 
            // m_pbEndR
            // 
            this.m_pbEndR.Location = new System.Drawing.Point(262, 42);
            this.m_pbEndR.Name = "m_pbEndR";
            this.m_pbEndR.Size = new System.Drawing.Size(28, 23);
            this.m_pbEndR.TabIndex = 24;
            this.m_pbEndR.Text = "<";
            this.m_pbEndR.UseVisualStyleBackColor = true;
            this.m_pbEndR.Click += new System.EventHandler(this.EndBumpRewind);
            // 
            // m_pbEndFR
            // 
            this.m_pbEndFR.Location = new System.Drawing.Point(238, 42);
            this.m_pbEndFR.Name = "m_pbEndFR";
            this.m_pbEndFR.Size = new System.Drawing.Size(28, 23);
            this.m_pbEndFR.TabIndex = 23;
            this.m_pbEndFR.Text = "<<";
            this.m_pbEndFR.UseVisualStyleBackColor = true;
            this.m_pbEndFR.Click += new System.EventHandler(this.EndBumpFastRewind);
            // 
            // m_pbStartR
            // 
            this.m_pbStartR.Location = new System.Drawing.Point(36, 42);
            this.m_pbStartR.Name = "m_pbStartR";
            this.m_pbStartR.Size = new System.Drawing.Size(28, 23);
            this.m_pbStartR.TabIndex = 26;
            this.m_pbStartR.Text = "<";
            this.m_pbStartR.UseVisualStyleBackColor = true;
            this.m_pbStartR.Click += new System.EventHandler(this.StartBumpReverse);
            // 
            // m_pbStartFR
            // 
            this.m_pbStartFR.Location = new System.Drawing.Point(12, 42);
            this.m_pbStartFR.Name = "m_pbStartFR";
            this.m_pbStartFR.Size = new System.Drawing.Size(28, 23);
            this.m_pbStartFR.TabIndex = 25;
            this.m_pbStartFR.Text = "<<";
            this.m_pbStartFR.UseVisualStyleBackColor = true;
            this.m_pbStartFR.Click += new System.EventHandler(this.StartBumpFastReverse);
            // 
            // AzLogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 623);
            this.Controls.Add(this.m_pbStartR);
            this.Controls.Add(this.m_pbStartFR);
            this.Controls.Add(this.m_pbEndR);
            this.Controls.Add(this.m_pbEndFR);
            this.Controls.Add(this.m_pbStartFF);
            this.Controls.Add(this.m_pbStartF);
            this.Controls.Add(this.m_pbEndFF);
            this.Controls.Add(this.m_pbEndF);
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
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HandleFormClosed);
            this.ctxMenuList.ResumeLayout(false);
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
        private System.Windows.Forms.ContextMenuStrip m_ctxmHeader;
        private System.Windows.Forms.ContextMenuStrip ctxMenuList;
        private System.Windows.Forms.ToolStripMenuItem tsmiFilter;
        private System.Windows.Forms.Button m_pbEndF;
        private System.Windows.Forms.Button m_pbEndFF;
        private System.Windows.Forms.Button m_pbStartFF;
        private System.Windows.Forms.Button m_pbStartF;
        private System.Windows.Forms.Button m_pbEndR;
        private System.Windows.Forms.Button m_pbEndFR;
        private System.Windows.Forms.Button m_pbStartR;
        private System.Windows.Forms.Button m_pbStartFR;
    }
}