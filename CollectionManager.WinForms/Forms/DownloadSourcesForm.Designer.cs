namespace GuiComponents.Forms
{
    partial class DownloadSourcesForm
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
            listBox_sources = new System.Windows.Forms.ListBox();
            textBox_sourceInfo = new System.Windows.Forms.TextBox();
            label_mirrors = new System.Windows.Forms.Label();
            listBox_mirrors = new System.Windows.Forms.ListBox();
            label_name = new System.Windows.Forms.Label();
            textBox_mirrorName = new System.Windows.Forms.TextBox();
            label_url = new System.Windows.Forms.Label();
            textBox_mirrorUrl = new System.Windows.Forms.TextBox();
            label_urlNoVideo = new System.Windows.Forms.Label();
            textBox_mirrorUrlNoVideo = new System.Windows.Forms.TextBox();
            label_referer = new System.Windows.Forms.Label();
            textBox_mirrorReferer = new System.Windows.Forms.TextBox();
            button_addMirror = new System.Windows.Forms.Button();
            button_removeMirror = new System.Windows.Forms.Button();
            button_moveUp = new System.Windows.Forms.Button();
            button_moveDown = new System.Windows.Forms.Button();
            button_save = new System.Windows.Forms.Button();
            button_close = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // listBox_sources
            // 
            listBox_sources.FormattingEnabled = true;
            listBox_sources.ItemHeight = 15;
            listBox_sources.Location = new System.Drawing.Point(12, 12);
            listBox_sources.Name = "listBox_sources";
            listBox_sources.Size = new System.Drawing.Size(210, 409);
            listBox_sources.TabIndex = 0;
            // 
            // textBox_sourceInfo
            // 
            textBox_sourceInfo.Location = new System.Drawing.Point(234, 12);
            textBox_sourceInfo.Multiline = true;
            textBox_sourceInfo.Name = "textBox_sourceInfo";
            textBox_sourceInfo.ReadOnly = true;
            textBox_sourceInfo.Size = new System.Drawing.Size(520, 96);
            textBox_sourceInfo.TabIndex = 1;
            // 
            // label_mirrors
            // 
            label_mirrors.AutoSize = true;
            label_mirrors.Location = new System.Drawing.Point(234, 121);
            label_mirrors.Name = "label_mirrors";
            label_mirrors.Size = new System.Drawing.Size(47, 15);
            label_mirrors.TabIndex = 2;
            label_mirrors.Text = "Mirrors:";
            // 
            // listBox_mirrors
            // 
            listBox_mirrors.FormattingEnabled = true;
            listBox_mirrors.ItemHeight = 15;
            listBox_mirrors.Location = new System.Drawing.Point(234, 139);
            listBox_mirrors.Name = "listBox_mirrors";
            listBox_mirrors.Size = new System.Drawing.Size(280, 214);
            listBox_mirrors.TabIndex = 3;
            // 
            // label_name
            // 
            label_name.AutoSize = true;
            label_name.Location = new System.Drawing.Point(530, 121);
            label_name.Name = "label_name";
            label_name.Size = new System.Drawing.Size(39, 15);
            label_name.TabIndex = 4;
            label_name.Text = "Name";
            // 
            // textBox_mirrorName
            // 
            textBox_mirrorName.Location = new System.Drawing.Point(530, 139);
            textBox_mirrorName.Name = "textBox_mirrorName";
            textBox_mirrorName.Size = new System.Drawing.Size(224, 23);
            textBox_mirrorName.TabIndex = 5;
            // 
            // label_url
            // 
            label_url.AutoSize = true;
            label_url.Location = new System.Drawing.Point(530, 170);
            label_url.Name = "label_url";
            label_url.Size = new System.Drawing.Size(61, 15);
            label_url.TabIndex = 6;
            label_url.Text = "Full URL";
            // 
            // textBox_mirrorUrl
            // 
            textBox_mirrorUrl.Location = new System.Drawing.Point(530, 188);
            textBox_mirrorUrl.Name = "textBox_mirrorUrl";
            textBox_mirrorUrl.Size = new System.Drawing.Size(224, 23);
            textBox_mirrorUrl.TabIndex = 7;
            // 
            // label_urlNoVideo
            // 
            label_urlNoVideo.AutoSize = true;
            label_urlNoVideo.Location = new System.Drawing.Point(530, 219);
            label_urlNoVideo.Name = "label_urlNoVideo";
            label_urlNoVideo.Size = new System.Drawing.Size(95, 15);
            label_urlNoVideo.TabIndex = 8;
            label_urlNoVideo.Text = "No-video URL";
            // 
            // textBox_mirrorUrlNoVideo
            // 
            textBox_mirrorUrlNoVideo.Location = new System.Drawing.Point(530, 237);
            textBox_mirrorUrlNoVideo.Name = "textBox_mirrorUrlNoVideo";
            textBox_mirrorUrlNoVideo.Size = new System.Drawing.Size(224, 23);
            textBox_mirrorUrlNoVideo.TabIndex = 9;
            // 
            // label_referer
            // 
            label_referer.AutoSize = true;
            label_referer.Location = new System.Drawing.Point(530, 268);
            label_referer.Name = "label_referer";
            label_referer.Size = new System.Drawing.Size(46, 15);
            label_referer.TabIndex = 10;
            label_referer.Text = "Referer";
            // 
            // textBox_mirrorReferer
            // 
            textBox_mirrorReferer.Location = new System.Drawing.Point(530, 286);
            textBox_mirrorReferer.Name = "textBox_mirrorReferer";
            textBox_mirrorReferer.Size = new System.Drawing.Size(224, 23);
            textBox_mirrorReferer.TabIndex = 11;
            // 
            // button_addMirror
            // 
            button_addMirror.Location = new System.Drawing.Point(234, 359);
            button_addMirror.Name = "button_addMirror";
            button_addMirror.Size = new System.Drawing.Size(90, 26);
            button_addMirror.TabIndex = 12;
            button_addMirror.Text = "Add";
            button_addMirror.UseVisualStyleBackColor = true;
            // 
            // button_removeMirror
            // 
            button_removeMirror.Location = new System.Drawing.Point(330, 359);
            button_removeMirror.Name = "button_removeMirror";
            button_removeMirror.Size = new System.Drawing.Size(90, 26);
            button_removeMirror.TabIndex = 13;
            button_removeMirror.Text = "Remove";
            button_removeMirror.UseVisualStyleBackColor = true;
            // 
            // button_moveUp
            // 
            button_moveUp.Location = new System.Drawing.Point(426, 359);
            button_moveUp.Name = "button_moveUp";
            button_moveUp.Size = new System.Drawing.Size(40, 26);
            button_moveUp.TabIndex = 14;
            button_moveUp.Text = "Up";
            button_moveUp.UseVisualStyleBackColor = true;
            // 
            // button_moveDown
            // 
            button_moveDown.Location = new System.Drawing.Point(472, 359);
            button_moveDown.Name = "button_moveDown";
            button_moveDown.Size = new System.Drawing.Size(42, 26);
            button_moveDown.TabIndex = 15;
            button_moveDown.Text = "Down";
            button_moveDown.UseVisualStyleBackColor = true;
            // 
            // button_save
            // 
            button_save.Location = new System.Drawing.Point(615, 359);
            button_save.Name = "button_save";
            button_save.Size = new System.Drawing.Size(139, 26);
            button_save.TabIndex = 16;
            button_save.Text = "Save";
            button_save.UseVisualStyleBackColor = true;
            // 
            // button_close
            // 
            button_close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button_close.Location = new System.Drawing.Point(680, 402);
            button_close.Name = "button_close";
            button_close.Size = new System.Drawing.Size(74, 26);
            button_close.TabIndex = 17;
            button_close.Text = "Close";
            button_close.UseVisualStyleBackColor = true;
            // 
            // DownloadSourcesForm
            // 
            AcceptButton = button_save;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = button_close;
            ClientSize = new System.Drawing.Size(766, 440);
            Controls.Add(button_close);
            Controls.Add(button_save);
            Controls.Add(button_moveDown);
            Controls.Add(button_moveUp);
            Controls.Add(button_removeMirror);
            Controls.Add(button_addMirror);
            Controls.Add(textBox_mirrorReferer);
            Controls.Add(label_referer);
            Controls.Add(textBox_mirrorUrlNoVideo);
            Controls.Add(label_urlNoVideo);
            Controls.Add(textBox_mirrorUrl);
            Controls.Add(label_url);
            Controls.Add(textBox_mirrorName);
            Controls.Add(label_name);
            Controls.Add(listBox_mirrors);
            Controls.Add(label_mirrors);
            Controls.Add(textBox_sourceInfo);
            Controls.Add(listBox_sources);
            Name = "DownloadSourcesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Download sources";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox listBox_sources;
        private System.Windows.Forms.TextBox textBox_sourceInfo;
        private System.Windows.Forms.Label label_mirrors;
        private System.Windows.Forms.ListBox listBox_mirrors;
        private System.Windows.Forms.Label label_name;
        private System.Windows.Forms.TextBox textBox_mirrorName;
        private System.Windows.Forms.Label label_url;
        private System.Windows.Forms.TextBox textBox_mirrorUrl;
        private System.Windows.Forms.Label label_urlNoVideo;
        private System.Windows.Forms.TextBox textBox_mirrorUrlNoVideo;
        private System.Windows.Forms.Label label_referer;
        private System.Windows.Forms.TextBox textBox_mirrorReferer;
        private System.Windows.Forms.Button button_addMirror;
        private System.Windows.Forms.Button button_removeMirror;
        private System.Windows.Forms.Button button_moveUp;
        private System.Windows.Forms.Button button_moveDown;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Button button_close;
    }
}