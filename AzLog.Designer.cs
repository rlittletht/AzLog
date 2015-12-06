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
            this.SuspendLayout();
            // 
            // m_ebStart
            // 
            this.m_ebStart.Location = new System.Drawing.Point(55, 280);
            this.m_ebStart.Name = "m_ebStart";
            this.m_ebStart.Size = new System.Drawing.Size(100, 20);
            this.m_ebStart.TabIndex = 6;
            this.m_ebStart.Text = "10/26/2015 8:00";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 283);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Start";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(171, 283);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "End";
            // 
            // m_ebEnd
            // 
            this.m_ebEnd.Location = new System.Drawing.Point(203, 280);
            this.m_ebEnd.Name = "m_ebEnd";
            this.m_ebEnd.Size = new System.Drawing.Size(100, 20);
            this.m_ebEnd.TabIndex = 8;
            this.m_ebEnd.Text = "10/26/2015 9:00";
            // 
            // m_pbFetch
            // 
            this.m_pbFetch.Location = new System.Drawing.Point(377, 277);
            this.m_pbFetch.Name = "m_pbFetch";
            this.m_pbFetch.Size = new System.Drawing.Size(105, 23);
            this.m_pbFetch.TabIndex = 10;
            this.m_pbFetch.Text = "Open Collection";
            this.m_pbFetch.UseVisualStyleBackColor = true;
            this.m_pbFetch.Click += new System.EventHandler(this.CreateViewForCollection);
            // 
            // m_cbxCollections
            // 
            this.m_cbxCollections.FormattingEnabled = true;
            this.m_cbxCollections.Location = new System.Drawing.Point(21, 28);
            this.m_cbxCollections.Name = "m_cbxCollections";
            this.m_cbxCollections.Size = new System.Drawing.Size(233, 21);
            this.m_cbxCollections.TabIndex = 13;
            this.m_cbxCollections.SelectedIndexChanged += new System.EventHandler(this.DoCollectionChanged);
            // 
            // m_pbAddDatasource
            // 
            this.m_pbAddDatasource.Location = new System.Drawing.Point(20, 212);
            this.m_pbAddDatasource.Name = "m_pbAddDatasource";
            this.m_pbAddDatasource.Size = new System.Drawing.Size(105, 21);
            this.m_pbAddDatasource.TabIndex = 14;
            this.m_pbAddDatasource.Text = "Add Datasource";
            this.m_pbAddDatasource.UseVisualStyleBackColor = true;
            this.m_pbAddDatasource.Click += new System.EventHandler(this.DoAddDatasource);
            // 
            // m_pbEditDatasource
            // 
            this.m_pbEditDatasource.Location = new System.Drawing.Point(20, 239);
            this.m_pbEditDatasource.Name = "m_pbEditDatasource";
            this.m_pbEditDatasource.Size = new System.Drawing.Size(105, 21);
            this.m_pbEditDatasource.TabIndex = 15;
            this.m_pbEditDatasource.Text = "Edit Datasource";
            this.m_pbEditDatasource.UseVisualStyleBackColor = true;
            // 
            // m_pbNewCollection
            // 
            this.m_pbNewCollection.Location = new System.Drawing.Point(266, 27);
            this.m_pbNewCollection.Name = "m_pbNewCollection";
            this.m_pbNewCollection.Size = new System.Drawing.Size(105, 21);
            this.m_pbNewCollection.TabIndex = 16;
            this.m_pbNewCollection.Text = "New Collection";
            this.m_pbNewCollection.UseVisualStyleBackColor = true;
            this.m_pbNewCollection.Click += new System.EventHandler(this.DoCreateCollection);
            // 
            // m_pbSaveCollection
            // 
            this.m_pbSaveCollection.Location = new System.Drawing.Point(377, 27);
            this.m_pbSaveCollection.Name = "m_pbSaveCollection";
            this.m_pbSaveCollection.Size = new System.Drawing.Size(105, 21);
            this.m_pbSaveCollection.TabIndex = 17;
            this.m_pbSaveCollection.Text = "Save Collection";
            this.m_pbSaveCollection.UseVisualStyleBackColor = true;
            this.m_pbSaveCollection.Click += new System.EventHandler(this.DoSaveCollection);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Available Datasources";
            // 
            // m_lbAvailableDatasources
            // 
            this.m_lbAvailableDatasources.FormattingEnabled = true;
            this.m_lbAvailableDatasources.Location = new System.Drawing.Point(20, 111);
            this.m_lbAvailableDatasources.Name = "m_lbAvailableDatasources";
            this.m_lbAvailableDatasources.Size = new System.Drawing.Size(120, 95);
            this.m_lbAvailableDatasources.TabIndex = 20;
            // 
            // m_lbCollectionSources
            // 
            this.m_lbCollectionSources.FormattingEnabled = true;
            this.m_lbCollectionSources.Location = new System.Drawing.Point(266, 111);
            this.m_lbCollectionSources.Name = "m_lbCollectionSources";
            this.m_lbCollectionSources.Size = new System.Drawing.Size(120, 95);
            this.m_lbCollectionSources.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(263, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Collection Sources";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(149, 137);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 21);
            this.button1.TabIndex = 23;
            this.button1.Text = ">> Include";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.DoIncludeDatasource);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(149, 164);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(105, 21);
            this.button2.TabIndex = 24;
            this.button2.Text = "Remove <<";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.DoRemoveDatasource);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(8, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(474, 19);
            this.label5.TabIndex = 25;
            this.label5.Tag = "Collection info";
            this.label5.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderHeadingLine);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(11, 263);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(471, 14);
            this.label6.TabIndex = 26;
            this.label6.Tag = "Execute query";
            this.label6.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderHeadingLine);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(8, 6);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(474, 19);
            this.label7.TabIndex = 27;
            this.label7.Tag = "Choose collection";
            this.label7.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderHeadingLine);
            // 
            // AzLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 311);
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
            this.Name = "AzLog";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(DoSaveState);

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
    }
}

