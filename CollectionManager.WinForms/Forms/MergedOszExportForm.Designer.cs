namespace GuiComponents.Forms
{
    partial class MergedOszExportForm
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
            groupBox_collection = new System.Windows.Forms.GroupBox();
            flowLayoutPanel_collection = new System.Windows.Forms.FlowLayoutPanel();
            label_collection = new System.Windows.Forms.Label();
            comboBox_collection = new System.Windows.Forms.ComboBox();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            label_source = new System.Windows.Forms.Label();
            listView_source = new System.Windows.Forms.ListView();
            columnHeader_sourceSong = new System.Windows.Forms.ColumnHeader();
            columnHeader_sourceDifficulty = new System.Windows.Forms.ColumnHeader();
            columnHeader_sourceMode = new System.Windows.Forms.ColumnHeader();
            label_export = new System.Windows.Forms.Label();
            flowLayoutPanel_moveButtons = new System.Windows.Forms.FlowLayoutPanel();
            button_add = new System.Windows.Forms.Button();
            button_remove = new System.Windows.Forms.Button();
            button_moveUp = new System.Windows.Forms.Button();
            button_moveDown = new System.Windows.Forms.Button();
            listView_export = new System.Windows.Forms.ListView();
            columnHeader_exportName = new System.Windows.Forms.ColumnHeader();
            columnHeader_exportSource = new System.Windows.Forms.ColumnHeader();
            groupBox_settings = new System.Windows.Forms.GroupBox();
            flowLayoutPanel_settings = new System.Windows.Forms.FlowLayoutPanel();
            label_packName = new System.Windows.Forms.Label();
            textBox_packName = new System.Windows.Forms.TextBox();
            label_creator = new System.Windows.Forms.Label();
            textBox_creator = new System.Windows.Forms.TextBox();
            label_extraTags = new System.Windows.Forms.Label();
            textBox_extraTags = new System.Windows.Forms.TextBox();
            label_outputDirectory = new System.Windows.Forms.Label();
            textBox_outputDirectory = new System.Windows.Forms.TextBox();
            button_browseOutputDirectory = new System.Windows.Forms.Button();
            button_export = new System.Windows.Forms.Button();
            button_close = new System.Windows.Forms.Button();
            groupBox_collection.SuspendLayout();
            flowLayoutPanel_collection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flowLayoutPanel_moveButtons.SuspendLayout();
            groupBox_settings.SuspendLayout();
            flowLayoutPanel_settings.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox_collection
            // 
            groupBox_collection.Controls.Add(flowLayoutPanel_collection);
            groupBox_collection.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox_collection.Location = new System.Drawing.Point(0, 0);
            groupBox_collection.Name = "groupBox_collection";
            groupBox_collection.Size = new System.Drawing.Size(984, 52);
            groupBox_collection.TabIndex = 0;
            groupBox_collection.TabStop = false;
            groupBox_collection.Text = "Collection";
            // 
            // flowLayoutPanel_collection
            // 
            flowLayoutPanel_collection.Controls.Add(label_collection);
            flowLayoutPanel_collection.Controls.Add(comboBox_collection);
            flowLayoutPanel_collection.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel_collection.Location = new System.Drawing.Point(3, 19);
            flowLayoutPanel_collection.Name = "flowLayoutPanel_collection";
            flowLayoutPanel_collection.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            flowLayoutPanel_collection.Size = new System.Drawing.Size(978, 30);
            flowLayoutPanel_collection.TabIndex = 0;
            // 
            // label_collection
            // 
            label_collection.AutoSize = true;
            label_collection.Location = new System.Drawing.Point(4, 7);
            label_collection.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            label_collection.Name = "label_collection";
            label_collection.Size = new System.Drawing.Size(41, 15);
            label_collection.TabIndex = 0;
            label_collection.Text = "Source:";
            // 
            // comboBox_collection
            // 
            comboBox_collection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox_collection.Location = new System.Drawing.Point(91, 4);
            comboBox_collection.Margin = new System.Windows.Forms.Padding(3, 4, 3, 3);
            comboBox_collection.Name = "comboBox_collection";
            comboBox_collection.Size = new System.Drawing.Size(520, 23);
            comboBox_collection.TabIndex = 1;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(0, 52);
            splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(label_source);
            splitContainer1.Panel1.Controls.Add(listView_source);
            splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(listView_export);
            splitContainer1.Panel2.Controls.Add(flowLayoutPanel_moveButtons);
            splitContainer1.Panel2.Controls.Add(label_export);
            splitContainer1.Panel2MinSize = 250;
            splitContainer1.Size = new System.Drawing.Size(984, 452);
            splitContainer1.SplitterDistance = 470;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 1;
            // 
            // label_source
            // 
            label_source.Dock = System.Windows.Forms.DockStyle.Top;
            label_source.Location = new System.Drawing.Point(0, 0);
            label_source.Name = "label_source";
            label_source.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            label_source.Size = new System.Drawing.Size(470, 20);
            label_source.TabIndex = 1;
            label_source.Text = "Beatmaps (select multiple, drag or use \"Add\"):";
            // 
            // listView_source
            // 
            listView_source.AllowDrop = true;
            listView_source.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader_sourceSong, columnHeader_sourceDifficulty, columnHeader_sourceMode });
            listView_source.Dock = System.Windows.Forms.DockStyle.Fill;
            listView_source.FullRowSelect = true;
            listView_source.HideSelection = false;
            listView_source.Location = new System.Drawing.Point(0, 20);
            listView_source.MultiSelect = true;
            listView_source.Name = "listView_source";
            listView_source.Size = new System.Drawing.Size(470, 432);
            listView_source.TabIndex = 0;
            listView_source.UseCompatibleStateImageBehavior = false;
            listView_source.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_sourceSong
            // 
            columnHeader_sourceSong.Text = "Artist - Title";
            columnHeader_sourceSong.Width = 260;
            // 
            // columnHeader_sourceDifficulty
            // 
            columnHeader_sourceDifficulty.Text = "Difficulty";
            columnHeader_sourceDifficulty.Width = 140;
            // 
            // columnHeader_sourceMode
            // 
            columnHeader_sourceMode.Text = "Mode";
            columnHeader_sourceMode.Width = 60;
            // 
            // label_export
            // 
            label_export.Dock = System.Windows.Forms.DockStyle.Top;
            label_export.Location = new System.Drawing.Point(0, 0);
            label_export.Name = "label_export";
            label_export.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            label_export.Size = new System.Drawing.Size(509, 20);
            label_export.TabIndex = 1;
            label_export.Text = "Export list (double-click name to edit, drag back to remove):";
            // 
            // flowLayoutPanel_moveButtons
            // 
            flowLayoutPanel_moveButtons.Controls.Add(button_add);
            flowLayoutPanel_moveButtons.Controls.Add(button_remove);
            flowLayoutPanel_moveButtons.Controls.Add(button_moveUp);
            flowLayoutPanel_moveButtons.Controls.Add(button_moveDown);
            flowLayoutPanel_moveButtons.Dock = System.Windows.Forms.DockStyle.Left;
            flowLayoutPanel_moveButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanel_moveButtons.Location = new System.Drawing.Point(0, 20);
            flowLayoutPanel_moveButtons.Name = "flowLayoutPanel_moveButtons";
            flowLayoutPanel_moveButtons.Padding = new System.Windows.Forms.Padding(4, 6, 4, 6);
            flowLayoutPanel_moveButtons.Size = new System.Drawing.Size(68, 432);
            flowLayoutPanel_moveButtons.TabIndex = 0;
            // 
            // button_add
            // 
            button_add.Location = new System.Drawing.Point(7, 9);
            button_add.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            button_add.Name = "button_add";
            button_add.Size = new System.Drawing.Size(54, 26);
            button_add.TabIndex = 0;
            button_add.Text = "Add ▸";
            button_add.UseVisualStyleBackColor = true;
            // 
            // button_remove
            // 
            button_remove.Location = new System.Drawing.Point(7, 41);
            button_remove.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            button_remove.Name = "button_remove";
            button_remove.Size = new System.Drawing.Size(54, 26);
            button_remove.TabIndex = 1;
            button_remove.Text = "◂ Remove";
            button_remove.UseVisualStyleBackColor = true;
            // 
            // button_moveUp
            // 
            button_moveUp.Location = new System.Drawing.Point(7, 73);
            button_moveUp.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            button_moveUp.Name = "button_moveUp";
            button_moveUp.Size = new System.Drawing.Size(54, 26);
            button_moveUp.TabIndex = 2;
            button_moveUp.Text = "▲ Up";
            button_moveUp.UseVisualStyleBackColor = true;
            // 
            // button_moveDown
            // 
            button_moveDown.Location = new System.Drawing.Point(7, 105);
            button_moveDown.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            button_moveDown.Name = "button_moveDown";
            button_moveDown.Size = new System.Drawing.Size(54, 26);
            button_moveDown.TabIndex = 3;
            button_moveDown.Text = "▼ Down";
            button_moveDown.UseVisualStyleBackColor = true;
            // 
            // listView_export
            // 
            listView_export.AllowDrop = true;
            listView_export.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader_exportName, columnHeader_exportSource });
            listView_export.Dock = System.Windows.Forms.DockStyle.Fill;
            listView_export.FullRowSelect = true;
            listView_export.HideSelection = false;
            listView_export.Location = new System.Drawing.Point(68, 20);
            listView_export.MultiSelect = true;
            listView_export.Name = "listView_export";
            listView_export.Size = new System.Drawing.Size(441, 432);
            listView_export.TabIndex = 2;
            listView_export.UseCompatibleStateImageBehavior = false;
            listView_export.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_exportName
            // 
            columnHeader_exportName.Text = "Version name (double-click to edit)";
            columnHeader_exportName.Width = 250;
            // 
            // columnHeader_exportSource
            // 
            columnHeader_exportSource.Text = "Source beatmap";
            columnHeader_exportSource.Width = 180;
            // 
            // groupBox_settings
            // 
            groupBox_settings.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox_settings.Controls.Add(flowLayoutPanel_settings);
            groupBox_settings.Location = new System.Drawing.Point(0, 504);
            groupBox_settings.Name = "groupBox_settings";
            groupBox_settings.Size = new System.Drawing.Size(984, 108);
            groupBox_settings.TabIndex = 2;
            groupBox_settings.TabStop = false;
            groupBox_settings.Text = "Pack settings";
            // 
            // flowLayoutPanel_settings
            // 
            flowLayoutPanel_settings.AutoScroll = true;
            flowLayoutPanel_settings.Controls.Add(label_packName);
            flowLayoutPanel_settings.Controls.Add(textBox_packName);
            flowLayoutPanel_settings.Controls.Add(label_creator);
            flowLayoutPanel_settings.Controls.Add(textBox_creator);
            flowLayoutPanel_settings.Controls.Add(label_extraTags);
            flowLayoutPanel_settings.Controls.Add(textBox_extraTags);
            flowLayoutPanel_settings.Controls.Add(label_outputDirectory);
            flowLayoutPanel_settings.Controls.Add(textBox_outputDirectory);
            flowLayoutPanel_settings.Controls.Add(button_browseOutputDirectory);
            flowLayoutPanel_settings.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel_settings.Location = new System.Drawing.Point(3, 19);
            flowLayoutPanel_settings.Name = "flowLayoutPanel_settings";
            flowLayoutPanel_settings.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);
            flowLayoutPanel_settings.Size = new System.Drawing.Size(978, 86);
            flowLayoutPanel_settings.TabIndex = 0;
            // 
            // label_packName
            // 
            label_packName.AutoSize = true;
            label_packName.Location = new System.Drawing.Point(7, 11);
            label_packName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            label_packName.Name = "label_packName";
            label_packName.Size = new System.Drawing.Size(63, 15);
            label_packName.TabIndex = 0;
            label_packName.Text = "Pack name:";
            // 
            // textBox_packName
            // 
            textBox_packName.Location = new System.Drawing.Point(76, 7);
            textBox_packName.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            textBox_packName.Name = "textBox_packName";
            textBox_packName.Size = new System.Drawing.Size(240, 23);
            textBox_packName.TabIndex = 1;
            // 
            // label_creator
            // 
            label_creator.AutoSize = true;
            label_creator.Location = new System.Drawing.Point(322, 11);
            label_creator.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            label_creator.Name = "label_creator";
            label_creator.Size = new System.Drawing.Size(50, 15);
            label_creator.TabIndex = 2;
            label_creator.Text = "Creator:";
            // 
            // textBox_creator
            // 
            textBox_creator.Location = new System.Drawing.Point(378, 7);
            textBox_creator.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            textBox_creator.Name = "textBox_creator";
            textBox_creator.Size = new System.Drawing.Size(180, 23);
            textBox_creator.TabIndex = 3;
            // 
            // label_extraTags
            // 
            label_extraTags.AutoSize = true;
            label_extraTags.Location = new System.Drawing.Point(564, 11);
            label_extraTags.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            label_extraTags.Name = "label_extraTags";
            label_extraTags.Size = new System.Drawing.Size(65, 15);
            label_extraTags.TabIndex = 4;
            label_extraTags.Text = "Extra tags:";
            // 
            // textBox_extraTags
            // 
            textBox_extraTags.Location = new System.Drawing.Point(635, 7);
            textBox_extraTags.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            textBox_extraTags.Name = "textBox_extraTags";
            textBox_extraTags.Size = new System.Drawing.Size(300, 23);
            textBox_extraTags.TabIndex = 5;
            textBox_extraTags.Text = "chordjack practice";
            // 
            // label_outputDirectory
            // 
            label_outputDirectory.AutoSize = true;
            label_outputDirectory.Location = new System.Drawing.Point(7, 42);
            label_outputDirectory.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            label_outputDirectory.Name = "label_outputDirectory";
            label_outputDirectory.Size = new System.Drawing.Size(96, 15);
            label_outputDirectory.TabIndex = 6;
            label_outputDirectory.Text = "Output directory:";
            // 
            // textBox_outputDirectory
            // 
            textBox_outputDirectory.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBox_outputDirectory.Location = new System.Drawing.Point(109, 38);
            textBox_outputDirectory.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            textBox_outputDirectory.Name = "textBox_outputDirectory";
            textBox_outputDirectory.Size = new System.Drawing.Size(560, 23);
            textBox_outputDirectory.TabIndex = 7;
            // 
            // button_browseOutputDirectory
            // 
            button_browseOutputDirectory.Location = new System.Drawing.Point(675, 37);
            button_browseOutputDirectory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            button_browseOutputDirectory.Name = "button_browseOutputDirectory";
            button_browseOutputDirectory.Size = new System.Drawing.Size(90, 24);
            button_browseOutputDirectory.TabIndex = 8;
            button_browseOutputDirectory.Text = "Browse...";
            button_browseOutputDirectory.UseVisualStyleBackColor = true;
            // 
            // button_export
            // 
            button_export.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button_export.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            button_export.Location = new System.Drawing.Point(872, 618);
            button_export.Name = "button_export";
            button_export.Size = new System.Drawing.Size(100, 30);
            button_export.TabIndex = 3;
            button_export.Text = "Export .osz";
            button_export.UseVisualStyleBackColor = true;
            // 
            // button_close
            // 
            button_close.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button_close.Location = new System.Drawing.Point(781, 618);
            button_close.Name = "button_close";
            button_close.Size = new System.Drawing.Size(85, 30);
            button_close.TabIndex = 4;
            button_close.Text = "Close";
            button_close.UseVisualStyleBackColor = true;
            // 
            // MergedOszExportForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(984, 660);
            Controls.Add(button_close);
            Controls.Add(button_export);
            Controls.Add(groupBox_settings);
            Controls.Add(splitContainer1);
            Controls.Add(groupBox_collection);
            MinimumSize = new System.Drawing.Size(820, 560);
            Name = "MergedOszExportForm";
            Text = "Export merged osz";
            groupBox_collection.ResumeLayout(false);
            flowLayoutPanel_collection.ResumeLayout(false);
            flowLayoutPanel_collection.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            flowLayoutPanel_moveButtons.ResumeLayout(false);
            groupBox_settings.ResumeLayout(false);
            flowLayoutPanel_settings.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_collection;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_collection;
        private System.Windows.Forms.Label label_collection;
        private System.Windows.Forms.ComboBox comboBox_collection;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label_source;
        private System.Windows.Forms.ListView listView_source;
        private System.Windows.Forms.ColumnHeader columnHeader_sourceSong;
        private System.Windows.Forms.ColumnHeader columnHeader_sourceDifficulty;
        private System.Windows.Forms.ColumnHeader columnHeader_sourceMode;
        private System.Windows.Forms.Label label_export;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_moveButtons;
        private System.Windows.Forms.Button button_add;
        private System.Windows.Forms.Button button_remove;
        private System.Windows.Forms.Button button_moveUp;
        private System.Windows.Forms.Button button_moveDown;
        private System.Windows.Forms.ListView listView_export;
        private System.Windows.Forms.ColumnHeader columnHeader_exportName;
        private System.Windows.Forms.ColumnHeader columnHeader_exportSource;
        private System.Windows.Forms.GroupBox groupBox_settings;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_settings;
        private System.Windows.Forms.Label label_packName;
        private System.Windows.Forms.TextBox textBox_packName;
        private System.Windows.Forms.Label label_creator;
        private System.Windows.Forms.TextBox textBox_creator;
        private System.Windows.Forms.Label label_extraTags;
        private System.Windows.Forms.TextBox textBox_extraTags;
        private System.Windows.Forms.Label label_outputDirectory;
        private System.Windows.Forms.TextBox textBox_outputDirectory;
        private System.Windows.Forms.Button button_browseOutputDirectory;
        private System.Windows.Forms.Button button_export;
        private System.Windows.Forms.Button button_close;
    }
}