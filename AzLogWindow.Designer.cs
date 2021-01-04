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
			this.m_lvLog = new TCore.UI.ListViewEx();
			this.m_ctxmListViewLog = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsmiFilter = new System.Windows.Forms.ToolStripMenuItem();
			this.filterOutThisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.colorThisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.blueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.redToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.yellowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.greenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.dToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cyanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.lightRedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.lightGreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.lightBlueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.blackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.lightYellowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
			this.label3 = new System.Windows.Forms.Label();
			this.m_pgbMain = new System.Windows.Forms.ProgressBar();
			this.label4 = new System.Windows.Forms.Label();
			this.m_cbFilters = new System.Windows.Forms.ComboBox();
			this.m_pbFilterSave = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.m_ctxmListViewLog.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_pbFetch
			// 
			this.m_pbFetch.Location = new System.Drawing.Point(662, 31);
			this.m_pbFetch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbFetch.Name = "m_pbFetch";
			this.m_pbFetch.Size = new System.Drawing.Size(112, 35);
			this.m_pbFetch.TabIndex = 11;
			this.m_pbFetch.Text = "Fetch";
			this.m_pbFetch.UseVisualStyleBackColor = true;
			this.m_pbFetch.Click += new System.EventHandler(this.DoFetch);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(486, 9);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(38, 20);
			this.label2.TabIndex = 13;
			this.label2.Text = "End";
			// 
			// m_ebEnd
			// 
			this.m_ebEnd.Location = new System.Drawing.Point(430, 34);
			this.m_ebEnd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_ebEnd.Name = "m_ebEnd";
			this.m_ebEnd.Size = new System.Drawing.Size(148, 26);
			this.m_ebEnd.TabIndex = 12;
			this.m_ebEnd.Text = "10/26/2015 9:00";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(114, 11);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 20);
			this.label1.TabIndex = 15;
			this.label1.Text = "Start";
			// 
			// m_ebStart
			// 
			this.m_ebStart.Location = new System.Drawing.Point(92, 34);
			this.m_ebStart.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_ebStart.Name = "m_ebStart";
			this.m_ebStart.Size = new System.Drawing.Size(148, 26);
			this.m_ebStart.TabIndex = 14;
			this.m_ebStart.Text = "10/26/2015 8:00";
			// 
			// m_lvLog
			// 
			this.m_lvLog.AllowColumnReorder = true;
			this.m_lvLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lvLog.ContextMenuStrip = this.m_ctxmListViewLog;
			this.m_lvLog.FullRowSelect = true;
			this.m_lvLog.HideSelection = false;
			this.m_lvLog.Location = new System.Drawing.Point(18, 75);
			this.m_lvLog.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_lvLog.Name = "m_lvLog";
			this.m_lvLog.Size = new System.Drawing.Size(1401, 852);
			this.m_lvLog.TabIndex = 16;
			this.m_lvLog.UseCompatibleStateImageBehavior = false;
			this.m_lvLog.View = System.Windows.Forms.View.Details;
			this.m_lvLog.VirtualMode = true;
			this.m_lvLog.ColumnReordered += new System.Windows.Forms.ColumnReorderedEventHandler(this.DoColumnReorder);
			this.m_lvLog.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.NotifyColumnWidthChanged);
			this.m_lvLog.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.GetListViewItem);
			// 
			// m_ctxmListViewLog
			// 
			this.m_ctxmListViewLog.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.m_ctxmListViewLog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFilter,
            this.filterOutThisToolStripMenuItem,
            this.colorThisToolStripMenuItem});
			this.m_ctxmListViewLog.Name = "ctxMenuList";
			this.m_ctxmListViewLog.Size = new System.Drawing.Size(200, 100);
			this.m_ctxmListViewLog.Opening += new System.ComponentModel.CancelEventHandler(this.HandleContextOpening);
			// 
			// tsmiFilter
			// 
			this.tsmiFilter.Name = "tsmiFilter";
			this.tsmiFilter.Size = new System.Drawing.Size(199, 32);
			this.tsmiFilter.Text = "Filter to this...";
			this.tsmiFilter.Click += new System.EventHandler(this.CreateFilterToContext);
			// 
			// filterOutThisToolStripMenuItem
			// 
			this.filterOutThisToolStripMenuItem.Name = "filterOutThisToolStripMenuItem";
			this.filterOutThisToolStripMenuItem.Size = new System.Drawing.Size(199, 32);
			this.filterOutThisToolStripMenuItem.Text = "Filter out this...";
			this.filterOutThisToolStripMenuItem.Click += new System.EventHandler(this.CreateFilterOutContext);
			// 
			// colorThisToolStripMenuItem
			// 
			this.colorThisToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
			this.colorThisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.blueToolStripMenuItem,
            this.redToolStripMenuItem,
            this.yellowToolStripMenuItem,
            this.greenToolStripMenuItem,
            this.dToolStripMenuItem,
            this.cyanToolStripMenuItem,
            this.lightRedToolStripMenuItem,
            this.lightGreenToolStripMenuItem,
            this.lightBlueToolStripMenuItem,
            this.blackToolStripMenuItem,
            this.lightYellowToolStripMenuItem});
			this.colorThisToolStripMenuItem.Name = "colorThisToolStripMenuItem";
			this.colorThisToolStripMenuItem.Size = new System.Drawing.Size(199, 32);
			this.colorThisToolStripMenuItem.Text = "Color this...";
			// 
			// blueToolStripMenuItem
			// 
			this.blueToolStripMenuItem.BackColor = System.Drawing.Color.Blue;
			this.blueToolStripMenuItem.ForeColor = System.Drawing.SystemColors.Control;
			this.blueToolStripMenuItem.Name = "blueToolStripMenuItem";
			this.blueToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.blueToolStripMenuItem.Text = "Blue";
			this.blueToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// redToolStripMenuItem
			// 
			this.redToolStripMenuItem.BackColor = System.Drawing.Color.Red;
			this.redToolStripMenuItem.Name = "redToolStripMenuItem";
			this.redToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.redToolStripMenuItem.Text = "Red";
			this.redToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// yellowToolStripMenuItem
			// 
			this.yellowToolStripMenuItem.BackColor = System.Drawing.Color.Yellow;
			this.yellowToolStripMenuItem.Name = "yellowToolStripMenuItem";
			this.yellowToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.yellowToolStripMenuItem.Text = "Yellow";
			this.yellowToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// greenToolStripMenuItem
			// 
			this.greenToolStripMenuItem.BackColor = System.Drawing.Color.Green;
			this.greenToolStripMenuItem.ForeColor = System.Drawing.SystemColors.Control;
			this.greenToolStripMenuItem.Name = "greenToolStripMenuItem";
			this.greenToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.greenToolStripMenuItem.Text = "Green";
			this.greenToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// dToolStripMenuItem
			// 
			this.dToolStripMenuItem.BackColor = System.Drawing.Color.Orange;
			this.dToolStripMenuItem.Name = "dToolStripMenuItem";
			this.dToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.dToolStripMenuItem.Text = "Orange";
			this.dToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// cyanToolStripMenuItem
			// 
			this.cyanToolStripMenuItem.BackColor = System.Drawing.Color.Cyan;
			this.cyanToolStripMenuItem.Name = "cyanToolStripMenuItem";
			this.cyanToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.cyanToolStripMenuItem.Text = "Cyan";
			this.cyanToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// lightRedToolStripMenuItem
			// 
			this.lightRedToolStripMenuItem.BackColor = System.Drawing.Color.Salmon;
			this.lightRedToolStripMenuItem.Name = "lightRedToolStripMenuItem";
			this.lightRedToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.lightRedToolStripMenuItem.Text = "Light Red";
			this.lightRedToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// lightGreenToolStripMenuItem
			// 
			this.lightGreenToolStripMenuItem.BackColor = System.Drawing.Color.LightGreen;
			this.lightGreenToolStripMenuItem.Name = "lightGreenToolStripMenuItem";
			this.lightGreenToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.lightGreenToolStripMenuItem.Text = "Light Green";
			this.lightGreenToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// lightBlueToolStripMenuItem
			// 
			this.lightBlueToolStripMenuItem.BackColor = System.Drawing.Color.LightBlue;
			this.lightBlueToolStripMenuItem.Name = "lightBlueToolStripMenuItem";
			this.lightBlueToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.lightBlueToolStripMenuItem.Text = "Light Blue";
			this.lightBlueToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// blackToolStripMenuItem
			// 
			this.blackToolStripMenuItem.BackColor = System.Drawing.Color.Black;
			this.blackToolStripMenuItem.ForeColor = System.Drawing.SystemColors.Control;
			this.blackToolStripMenuItem.Name = "blackToolStripMenuItem";
			this.blackToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.blackToolStripMenuItem.Text = "Black";
			this.blackToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// lightYellowToolStripMenuItem
			// 
			this.lightYellowToolStripMenuItem.BackColor = System.Drawing.Color.LightYellow;
			this.lightYellowToolStripMenuItem.Name = "lightYellowToolStripMenuItem";
			this.lightYellowToolStripMenuItem.Size = new System.Drawing.Size(207, 34);
			this.lightYellowToolStripMenuItem.Text = "Light Yellow";
			this.lightYellowToolStripMenuItem.Click += new System.EventHandler(this.CreateColorContext);
			// 
			// m_cbView
			// 
			this.m_cbView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbView.FormattingEnabled = true;
			this.m_cbView.Location = new System.Drawing.Point(862, 34);
			this.m_cbView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_cbView.Name = "m_cbView";
			this.m_cbView.Size = new System.Drawing.Size(180, 28);
			this.m_cbView.TabIndex = 17;
			this.m_cbView.SelectedIndexChanged += new System.EventHandler(this.ChangeViewSelected);
			// 
			// m_pbSave
			// 
			this.m_pbSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.m_pbSave.Font = new System.Drawing.Font("Segoe UI Symbol", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_pbSave.Location = new System.Drawing.Point(1050, 31);
			this.m_pbSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbSave.Name = "m_pbSave";
			this.m_pbSave.Size = new System.Drawing.Size(36, 35);
			this.m_pbSave.TabIndex = 18;
			this.m_pbSave.Text = "💾";
			this.m_pbSave.UseVisualStyleBackColor = true;
			this.m_pbSave.Click += new System.EventHandler(this.DoViewSave);
			// 
			// m_ctxmHeader
			// 
			this.m_ctxmHeader.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.m_ctxmHeader.Name = "m_ctxmHeader";
			this.m_ctxmHeader.Size = new System.Drawing.Size(61, 4);
			// 
			// m_pbEndF
			// 
			this.m_pbEndF.Location = new System.Drawing.Point(573, 31);
			this.m_pbEndF.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbEndF.Name = "m_pbEndF";
			this.m_pbEndF.Size = new System.Drawing.Size(42, 35);
			this.m_pbEndF.TabIndex = 19;
			this.m_pbEndF.Text = ">";
			this.m_pbEndF.UseVisualStyleBackColor = true;
			this.m_pbEndF.Click += new System.EventHandler(this.EndBumpForward);
			// 
			// m_pbEndFF
			// 
			this.m_pbEndFF.Location = new System.Drawing.Point(610, 31);
			this.m_pbEndFF.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbEndFF.Name = "m_pbEndFF";
			this.m_pbEndFF.Size = new System.Drawing.Size(42, 35);
			this.m_pbEndFF.TabIndex = 20;
			this.m_pbEndFF.Text = ">>";
			this.m_pbEndFF.UseVisualStyleBackColor = true;
			this.m_pbEndFF.Click += new System.EventHandler(this.EndBumpFastForward);
			// 
			// m_pbStartFF
			// 
			this.m_pbStartFF.Location = new System.Drawing.Point(274, 31);
			this.m_pbStartFF.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbStartFF.Name = "m_pbStartFF";
			this.m_pbStartFF.Size = new System.Drawing.Size(42, 35);
			this.m_pbStartFF.TabIndex = 22;
			this.m_pbStartFF.Text = ">>";
			this.m_pbStartFF.UseVisualStyleBackColor = true;
			this.m_pbStartFF.Click += new System.EventHandler(this.StartBumpFastForward);
			// 
			// m_pbStartF
			// 
			this.m_pbStartF.Location = new System.Drawing.Point(237, 31);
			this.m_pbStartF.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbStartF.Name = "m_pbStartF";
			this.m_pbStartF.Size = new System.Drawing.Size(42, 35);
			this.m_pbStartF.TabIndex = 21;
			this.m_pbStartF.Text = ">";
			this.m_pbStartF.UseVisualStyleBackColor = true;
			this.m_pbStartF.Click += new System.EventHandler(this.StartBumpForward);
			// 
			// m_pbEndR
			// 
			this.m_pbEndR.Location = new System.Drawing.Point(390, 31);
			this.m_pbEndR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbEndR.Name = "m_pbEndR";
			this.m_pbEndR.Size = new System.Drawing.Size(42, 35);
			this.m_pbEndR.TabIndex = 24;
			this.m_pbEndR.Text = "<";
			this.m_pbEndR.UseVisualStyleBackColor = true;
			this.m_pbEndR.Click += new System.EventHandler(this.EndBumpRewind);
			// 
			// m_pbEndFR
			// 
			this.m_pbEndFR.Location = new System.Drawing.Point(354, 31);
			this.m_pbEndFR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbEndFR.Name = "m_pbEndFR";
			this.m_pbEndFR.Size = new System.Drawing.Size(42, 35);
			this.m_pbEndFR.TabIndex = 23;
			this.m_pbEndFR.Text = "<<";
			this.m_pbEndFR.UseVisualStyleBackColor = true;
			this.m_pbEndFR.Click += new System.EventHandler(this.EndBumpFastRewind);
			// 
			// m_pbStartR
			// 
			this.m_pbStartR.Location = new System.Drawing.Point(51, 31);
			this.m_pbStartR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbStartR.Name = "m_pbStartR";
			this.m_pbStartR.Size = new System.Drawing.Size(42, 35);
			this.m_pbStartR.TabIndex = 26;
			this.m_pbStartR.Text = "<";
			this.m_pbStartR.UseVisualStyleBackColor = true;
			this.m_pbStartR.Click += new System.EventHandler(this.StartBumpReverse);
			// 
			// m_pbStartFR
			// 
			this.m_pbStartFR.Location = new System.Drawing.Point(15, 31);
			this.m_pbStartFR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbStartFR.Name = "m_pbStartFR";
			this.m_pbStartFR.Size = new System.Drawing.Size(42, 35);
			this.m_pbStartFR.TabIndex = 25;
			this.m_pbStartFR.Text = "<<";
			this.m_pbStartFR.UseVisualStyleBackColor = true;
			this.m_pbStartFR.Click += new System.EventHandler(this.StartBumpFastReverse);
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(807, 38);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(47, 20);
			this.label3.TabIndex = 27;
			this.label3.Text = "View:";
			// 
			// m_pgbMain
			// 
			this.m_pgbMain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pgbMain.Location = new System.Drawing.Point(1157, 938);
			this.m_pgbMain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pgbMain.MarqueeAnimationSpeed = 30;
			this.m_pgbMain.Name = "m_pgbMain";
			this.m_pgbMain.Size = new System.Drawing.Size(264, 34);
			this.m_pgbMain.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.m_pgbMain.TabIndex = 31;
			this.m_pgbMain.Visible = false;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(1093, 39);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 20);
			this.label4.TabIndex = 33;
			this.label4.Text = "Filters:";
			// 
			// m_cbFilters
			// 
			this.m_cbFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbFilters.FormattingEnabled = true;
			this.m_cbFilters.Location = new System.Drawing.Point(1154, 35);
			this.m_cbFilters.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_cbFilters.Name = "m_cbFilters";
			this.m_cbFilters.Size = new System.Drawing.Size(180, 28);
			this.m_cbFilters.TabIndex = 32;
			this.m_cbFilters.SelectedIndexChanged += new System.EventHandler(this.ChangeFilterSelected);
			// 
			// m_pbFilterSave
			// 
			this.m_pbFilterSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbFilterSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.m_pbFilterSave.Font = new System.Drawing.Font("Segoe UI Symbol", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_pbFilterSave.Location = new System.Drawing.Point(1380, 30);
			this.m_pbFilterSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.m_pbFilterSave.Name = "m_pbFilterSave";
			this.m_pbFilterSave.Size = new System.Drawing.Size(36, 35);
			this.m_pbFilterSave.TabIndex = 34;
			this.m_pbFilterSave.Text = "💾";
			this.m_pbFilterSave.UseVisualStyleBackColor = true;
			this.m_pbFilterSave.Click += new System.EventHandler(this.DoFilterSave);
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button2.Font = new System.Drawing.Font("Segoe UI Symbol", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button2.Location = new System.Drawing.Point(1342, 30);
			this.button2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(36, 35);
			this.button2.TabIndex = 35;
			this.button2.Text = "✎";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.DoEditRemoveFilters);
			// 
			// AzLogWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1439, 974);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.m_pbFilterSave);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.m_cbFilters);
			this.Controls.Add(this.m_pgbMain);
			this.Controls.Add(this.label3);
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
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "AzLogWindow";
			this.Text = "AzLogWindow";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HandleFormClosed);
			this.m_ctxmListViewLog.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_pbFetch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox m_ebEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox m_ebStart;
        private TCore.UI.ListViewEx m_lvLog;
        private System.Windows.Forms.ComboBox m_cbView;
        private System.Windows.Forms.Button m_pbSave;
        private System.Windows.Forms.ContextMenuStrip m_ctxmHeader;
        private System.Windows.Forms.ContextMenuStrip m_ctxmListViewLog;
        private System.Windows.Forms.ToolStripMenuItem tsmiFilter;
        private System.Windows.Forms.Button m_pbEndF;
        private System.Windows.Forms.Button m_pbEndFF;
        private System.Windows.Forms.Button m_pbStartFF;
        private System.Windows.Forms.Button m_pbStartF;
        private System.Windows.Forms.Button m_pbEndR;
        private System.Windows.Forms.Button m_pbEndFR;
        private System.Windows.Forms.Button m_pbStartR;
        private System.Windows.Forms.Button m_pbStartFR;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar m_pgbMain;
        private System.Windows.Forms.ToolStripMenuItem filterOutThisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorThisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yellowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem greenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cyanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightRedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightGreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightBlueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightYellowToolStripMenuItem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox m_cbFilters;
        private System.Windows.Forms.Button m_pbFilterSave;
        private System.Windows.Forms.Button button2;
    }
}