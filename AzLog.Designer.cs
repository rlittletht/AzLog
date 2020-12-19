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
            this.m_ebStart = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_ebEnd = new System.Windows.Forms.TextBox();
            this.m_pbFetch = new System.Windows.Forms.Button();
            this.m_cbxCollections = new System.Windows.Forms.ComboBox();
            this.m_pbAddDatasource = new System.Windows.Forms.Button();
            this.m_pbEditDatasource = new System.Windows.Forms.Button();
            this.m_pbNewCollection = new System.Windows.Forms.Button();
            this.m_pbSaveCollection = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.m_lbAvailableDatasources = new System.Windows.Forms.ListBox();
            this.m_lbCollectionSources = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.m_cbAllDates = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // m_ebStart
            // 
            this.m_ebStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_ebStart.Location = new System.Drawing.Point(82, 535);
            this.m_ebStart.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_ebStart.Name = "m_ebStart";
            this.m_ebStart.Size = new System.Drawing.Size(148, 26);
            this.m_ebStart.TabIndex = 6;
            this.m_ebStart.Text = "10/26/2015 8:00";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 539);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Start";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(256, 539);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 20);
            this.label2.TabIndex = 9;
            this.label2.Text = "End";
            // 
            // m_ebEnd
            // 
            this.m_ebEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_ebEnd.Location = new System.Drawing.Point(304, 535);
            this.m_ebEnd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_ebEnd.Name = "m_ebEnd";
            this.m_ebEnd.Size = new System.Drawing.Size(148, 26);
            this.m_ebEnd.TabIndex = 8;
            this.m_ebEnd.Text = "10/26/2015 9:00";
            // 
            // m_pbFetch
            // 
            this.m_pbFetch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbFetch.Location = new System.Drawing.Point(709, 530);
            this.m_pbFetch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_pbFetch.Name = "m_pbFetch";
            this.m_pbFetch.Size = new System.Drawing.Size(158, 35);
            this.m_pbFetch.TabIndex = 10;
            this.m_pbFetch.Text = "Open Collection";
            this.m_pbFetch.UseVisualStyleBackColor = true;
            this.m_pbFetch.Click += new System.EventHandler(this.CreateViewForCollection);
            // 
            // m_cbxCollections
            // 
            this.m_cbxCollections.FormattingEnabled = true;
            this.m_cbxCollections.Location = new System.Drawing.Point(32, 43);
            this.m_cbxCollections.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbxCollections.Name = "m_cbxCollections";
            this.m_cbxCollections.Size = new System.Drawing.Size(348, 28);
            this.m_cbxCollections.TabIndex = 13;
            this.m_cbxCollections.SelectedIndexChanged += new System.EventHandler(this.DoCollectionChanged);
            // 
            // m_pbAddDatasource
            // 
            this.m_pbAddDatasource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_pbAddDatasource.Location = new System.Drawing.Point(30, 430);
            this.m_pbAddDatasource.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_pbAddDatasource.Name = "m_pbAddDatasource";
            this.m_pbAddDatasource.Size = new System.Drawing.Size(158, 32);
            this.m_pbAddDatasource.TabIndex = 14;
            this.m_pbAddDatasource.Text = "Add Datasource";
            this.m_pbAddDatasource.UseVisualStyleBackColor = true;
            this.m_pbAddDatasource.Click += new System.EventHandler(this.DoAddDatasource);
            // 
            // m_pbEditDatasource
            // 
            this.m_pbEditDatasource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_pbEditDatasource.Location = new System.Drawing.Point(30, 472);
            this.m_pbEditDatasource.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_pbEditDatasource.Name = "m_pbEditDatasource";
            this.m_pbEditDatasource.Size = new System.Drawing.Size(158, 32);
            this.m_pbEditDatasource.TabIndex = 15;
            this.m_pbEditDatasource.Text = "Edit Datasource";
            this.m_pbEditDatasource.UseVisualStyleBackColor = true;
            this.m_pbEditDatasource.Click += new System.EventHandler(this.DoEditDatasource);
            // 
            // m_pbNewCollection
            // 
            this.m_pbNewCollection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbNewCollection.Location = new System.Drawing.Point(542, 42);
            this.m_pbNewCollection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_pbNewCollection.Name = "m_pbNewCollection";
            this.m_pbNewCollection.Size = new System.Drawing.Size(158, 32);
            this.m_pbNewCollection.TabIndex = 16;
            this.m_pbNewCollection.Text = "New Collection";
            this.m_pbNewCollection.UseVisualStyleBackColor = true;
            this.m_pbNewCollection.Click += new System.EventHandler(this.DoCreateCollection);
            // 
            // m_pbSaveCollection
            // 
            this.m_pbSaveCollection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbSaveCollection.Location = new System.Drawing.Point(709, 42);
            this.m_pbSaveCollection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_pbSaveCollection.Name = "m_pbSaveCollection";
            this.m_pbSaveCollection.Size = new System.Drawing.Size(158, 32);
            this.m_pbSaveCollection.TabIndex = 17;
            this.m_pbSaveCollection.Text = "Save Collection";
            this.m_pbSaveCollection.UseVisualStyleBackColor = true;
            this.m_pbSaveCollection.Click += new System.EventHandler(this.DoSaveCollection);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 146);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(167, 20);
            this.label3.TabIndex = 19;
            this.label3.Text = "Available Datasources";
            // 
            // m_lbAvailableDatasources
            // 
            this.m_lbAvailableDatasources.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lbAvailableDatasources.FormattingEnabled = true;
            this.m_lbAvailableDatasources.ItemHeight = 20;
            this.m_lbAvailableDatasources.Location = new System.Drawing.Point(30, 171);
            this.m_lbAvailableDatasources.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lbAvailableDatasources.Name = "m_lbAvailableDatasources";
            this.m_lbAvailableDatasources.Size = new System.Drawing.Size(321, 244);
            this.m_lbAvailableDatasources.TabIndex = 20;
            // 
            // m_lbCollectionSources
            // 
            this.m_lbCollectionSources.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lbCollectionSources.FormattingEnabled = true;
            this.m_lbCollectionSources.ItemHeight = 20;
            this.m_lbCollectionSources.Location = new System.Drawing.Point(525, 171);
            this.m_lbCollectionSources.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lbCollectionSources.Name = "m_lbCollectionSources";
            this.m_lbCollectionSources.Size = new System.Drawing.Size(342, 244);
            this.m_lbCollectionSources.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(394, 146);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(141, 20);
            this.label4.TabIndex = 22;
            this.label4.Text = "Collection Sources";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(359, 195);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(158, 32);
            this.button1.TabIndex = 23;
            this.button1.Text = ">> Include";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.DoIncludeDatasource);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(359, 237);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(158, 32);
            this.button2.TabIndex = 24;
            this.button2.Text = "Remove <<";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.DoRemoveDatasource);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(12, 97);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(855, 29);
            this.label5.TabIndex = 25;
            this.label5.Tag = "Collection info";
            this.label5.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderHeadingLine);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.Location = new System.Drawing.Point(16, 509);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(849, 22);
            this.label6.TabIndex = 26;
            this.label6.Tag = "Execute query";
            this.label6.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderHeadingLine);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(12, 9);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(856, 29);
            this.label7.TabIndex = 27;
            this.label7.Tag = "Choose collection";
            this.label7.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderHeadingLine);
            // 
            // m_cbAllDates
            // 
            this.m_cbAllDates.AutoSize = true;
            this.m_cbAllDates.Location = new System.Drawing.Point(482, 534);
            this.m_cbAllDates.Name = "m_cbAllDates";
            this.m_cbAllDates.Size = new System.Drawing.Size(52, 24);
            this.m_cbAllDates.TabIndex = 28;
            this.m_cbAllDates.Text = "All";
            this.m_cbAllDates.UseVisualStyleBackColor = true;
            this.m_cbAllDates.CheckedChanged += new System.EventHandler(this.OnAllDatesCheckedChanged);
            // 
            // AzLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 582);
            this.Controls.Add(this.m_cbAllDates);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.m_lbCollectionSources);
            this.Controls.Add(this.m_lbAvailableDatasources);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.m_pbSaveCollection);
            this.Controls.Add(this.m_pbNewCollection);
            this.Controls.Add(this.m_pbEditDatasource);
            this.Controls.Add(this.m_pbAddDatasource);
            this.Controls.Add(this.m_cbxCollections);
            this.Controls.Add(this.m_pbFetch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_ebEnd);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_ebStart);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "AzLog";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DoSaveState);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox m_ebStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox m_ebEnd;
        private System.Windows.Forms.Button m_pbFetch;
        private System.Windows.Forms.ComboBox m_cbxCollections;
        private System.Windows.Forms.Button m_pbAddDatasource;
        private System.Windows.Forms.Button m_pbEditDatasource;
        private System.Windows.Forms.Button m_pbNewCollection;
        private System.Windows.Forms.Button m_pbSaveCollection;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox m_lbAvailableDatasources;
        private System.Windows.Forms.ListBox m_lbCollectionSources;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox m_cbAllDates;
    }
}

