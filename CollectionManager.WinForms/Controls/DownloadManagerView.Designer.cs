namespace GuiComponents.Controls
{
    partial class DownloadManagerView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ListViewDownload = new BrightIdeasSoftware.FastObjectListView();
            olvColumn_id = new BrightIdeasSoftware.OLVColumn();
            olvColumn_name = new BrightIdeasSoftware.OLVColumn();
            olvColumn_progress = new BrightIdeasSoftware.OLVColumn();
            olvColumn_speed = new BrightIdeasSoftware.OLVColumn();
            olvColumn_mirror = new BrightIdeasSoftware.OLVColumn();
            olvColumn_status = new BrightIdeasSoftware.OLVColumn();
            button_ToggleDownloads = new System.Windows.Forms.Button();
            label_status = new System.Windows.Forms.Label();
            label_source = new System.Windows.Forms.Label();
            comboBox_source = new System.Windows.Forms.ComboBox();
            contextMenuStrip_downloads = new System.Windows.Forms.ContextMenuStrip(components);
            menuItem_pause = new System.Windows.Forms.ToolStripMenuItem();
            menuItem_resume = new System.Windows.Forms.ToolStripMenuItem();
            menuItem_remove = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            menuItem_retry = new System.Windows.Forms.ToolStripMenuItem();
            menuItem_switchMirror = new System.Windows.Forms.ToolStripMenuItem();
            menuItem_mirror = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)ListViewDownload).BeginInit();
            contextMenuStrip_downloads.SuspendLayout();
            SuspendLayout();
            // 
            // ListViewDownload
            // 
            ListViewDownload.AllColumns.Add(olvColumn_id);
            ListViewDownload.AllColumns.Add(olvColumn_name);
            ListViewDownload.AllColumns.Add(olvColumn_progress);
            ListViewDownload.AllColumns.Add(olvColumn_speed);
            ListViewDownload.AllColumns.Add(olvColumn_mirror);
            ListViewDownload.AllColumns.Add(olvColumn_status);
            ListViewDownload.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ListViewDownload.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvColumn_id, olvColumn_name, olvColumn_progress, olvColumn_speed, olvColumn_mirror, olvColumn_status });
            ListViewDownload.ContextMenuStrip = contextMenuStrip_downloads;
            ListViewDownload.Location = new System.Drawing.Point(0, 37);
            ListViewDownload.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ListViewDownload.Name = "ListViewDownload";
            ListViewDownload.ShowGroups = false;
            ListViewDownload.Size = new System.Drawing.Size(900, 378);
            ListViewDownload.TabIndex = 1;
            ListViewDownload.UnfocusedHighlightBackgroundColor = System.Drawing.Color.FromArgb(192, 255, 192);
            ListViewDownload.UseCompatibleStateImageBehavior = false;
            ListViewDownload.UseCustomSelectionColors = true;
            ListViewDownload.UseNotifyPropertyChanged = true;
            ListViewDownload.View = System.Windows.Forms.View.Details;
            ListViewDownload.VirtualMode = true;
            // 
            // olvColumn_id
            // 
            olvColumn_id.AspectName = "Id";
            olvColumn_id.Text = "ID";
            olvColumn_id.Width = 40;
            // 
            // olvColumn_name
            // 
            olvColumn_name.AspectName = "Name";
            olvColumn_name.Text = "Beatmap";
            olvColumn_name.Width = 250;
            // 
            // olvColumn_progress
            // 
            olvColumn_progress.AspectName = "Progress";
            olvColumn_progress.Text = "Progress";
            olvColumn_progress.Width = 190;
            // 
            // olvColumn_speed
            // 
            olvColumn_speed.AspectName = "SpeedText";
            olvColumn_speed.Text = "Speed";
            olvColumn_speed.Width = 80;
            // 
            // olvColumn_mirror
            // 
            olvColumn_mirror.AspectName = "CurrentMirrorName";
            olvColumn_mirror.Text = "Mirror";
            olvColumn_mirror.Width = 110;
            // 
            // olvColumn_status
            // 
            olvColumn_status.AspectName = "Status";
            olvColumn_status.Text = "Status";
            olvColumn_status.Width = 100;
            // 
            // button_ToggleDownloads
            // 
            button_ToggleDownloads.Location = new System.Drawing.Point(4, 3);
            button_ToggleDownloads.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button_ToggleDownloads.Name = "button_ToggleDownloads";
            button_ToggleDownloads.Size = new System.Drawing.Size(183, 27);
            button_ToggleDownloads.TabIndex = 3;
            button_ToggleDownloads.Text = "Stop downloads";
            button_ToggleDownloads.UseVisualStyleBackColor = true;
            // 
            // label_status
            // 
            label_status.AutoSize = true;
            label_status.Location = new System.Drawing.Point(194, 9);
            label_status.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label_status.Name = "label_status";
            label_status.Size = new System.Drawing.Size(34, 15);
            label_status.TabIndex = 4;
            label_status.Text = "         ";
            // 
            // label_source
            // 
            label_source.AutoSize = true;
            label_source.Location = new System.Drawing.Point(300, 9);
            label_source.Name = "label_source";
            label_source.Size = new System.Drawing.Size(41, 15);
            label_source.TabIndex = 5;
            label_source.Text = "Source:";
            // 
            // comboBox_source
            // 
            comboBox_source.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            comboBox_source.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox_source.FormattingEnabled = true;
            comboBox_source.Location = new System.Drawing.Point(340, 5);
            comboBox_source.Name = "comboBox_source";
            comboBox_source.Size = new System.Drawing.Size(232, 23);
            comboBox_source.TabIndex = 6;
            // 
            // contextMenuStrip_downloads
            // 
            contextMenuStrip_downloads.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuItem_pause, menuItem_resume, menuItem_remove, toolStripSeparator1, menuItem_retry, menuItem_switchMirror, menuItem_mirror });
            contextMenuStrip_downloads.Name = "contextMenuStrip_downloads";
            contextMenuStrip_downloads.Size = new System.Drawing.Size(171, 104);
            // 
            // menuItem_pause
            // 
            menuItem_pause.Name = "menuItem_pause";
            menuItem_pause.Size = new System.Drawing.Size(170, 22);
            menuItem_pause.Text = "Pause";
            // 
            // menuItem_resume
            // 
            menuItem_resume.Name = "menuItem_resume";
            menuItem_resume.Size = new System.Drawing.Size(170, 22);
            menuItem_resume.Text = "Resume";
            // 
            // menuItem_remove
            // 
            menuItem_remove.Name = "menuItem_remove";
            menuItem_remove.Size = new System.Drawing.Size(170, 22);
            menuItem_remove.Text = "Remove";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(167, 6);
            // 
            // menuItem_retry
            // 
            menuItem_retry.Name = "menuItem_retry";
            menuItem_retry.Size = new System.Drawing.Size(170, 22);
            menuItem_retry.Text = "Retry";
            // 
            // menuItem_switchMirror
            // 
            menuItem_switchMirror.Name = "menuItem_switchMirror";
            menuItem_switchMirror.Size = new System.Drawing.Size(170, 22);
            menuItem_switchMirror.Text = "Switch mirror";
            // 
            // menuItem_mirror
            // 
            menuItem_mirror.Name = "menuItem_mirror";
            menuItem_mirror.Size = new System.Drawing.Size(170, 22);
            menuItem_mirror.Text = "Mirror";
            // 
            // DownloadManagerView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(label_status);
            Controls.Add(button_ToggleDownloads);
            Controls.Add(comboBox_source);
            Controls.Add(label_source);
            Controls.Add(ListViewDownload);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "DownloadManagerView";
            Size = new System.Drawing.Size(901, 415);
            ((System.ComponentModel.ISupportInitialize)ListViewDownload).EndInit();
            contextMenuStrip_downloads.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.FastObjectListView ListViewDownload;
        private BrightIdeasSoftware.OLVColumn olvColumn_id;
        private BrightIdeasSoftware.OLVColumn olvColumn_name;
        private BrightIdeasSoftware.OLVColumn olvColumn_progress;
        private BrightIdeasSoftware.OLVColumn olvColumn_speed;
        private BrightIdeasSoftware.OLVColumn olvColumn_mirror;
        private BrightIdeasSoftware.OLVColumn olvColumn_status;
        private System.Windows.Forms.Button button_ToggleDownloads;
        private System.Windows.Forms.Label label_status;
        private System.Windows.Forms.Label label_source;
        private System.Windows.Forms.ComboBox comboBox_source;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_downloads;
        private System.Windows.Forms.ToolStripMenuItem menuItem_pause;
        private System.Windows.Forms.ToolStripMenuItem menuItem_resume;
        private System.Windows.Forms.ToolStripMenuItem menuItem_remove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuItem_retry;
        private System.Windows.Forms.ToolStripMenuItem menuItem_switchMirror;
        private System.Windows.Forms.ToolStripMenuItem menuItem_mirror;
    }
}