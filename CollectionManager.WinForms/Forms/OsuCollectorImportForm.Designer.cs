namespace GuiComponents.Forms
{
    partial class OsuCollectorImportForm
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
            label_linkInfo = new System.Windows.Forms.Label();
            textBox_link = new System.Windows.Forms.TextBox();
            button_import = new System.Windows.Forms.Button();
            label_behavior = new System.Windows.Forms.Label();
            comboBox_behavior = new System.Windows.Forms.ComboBox();
            textBox_summary = new System.Windows.Forms.TextBox();
            button_close = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // label_linkInfo
            // 
            label_linkInfo.AutoSize = true;
            label_linkInfo.Location = new System.Drawing.Point(12, 12);
            label_linkInfo.Name = "label_linkInfo";
            label_linkInfo.Size = new System.Drawing.Size(318, 15);
            label_linkInfo.TabIndex = 0;
            label_linkInfo.Text = "Paste an osu!collector collection link or ID (e.g. 23199):";
            // 
            // textBox_link
            // 
            textBox_link.Location = new System.Drawing.Point(12, 33);
            textBox_link.Name = "textBox_link";
            textBox_link.Size = new System.Drawing.Size(416, 23);
            textBox_link.TabIndex = 1;
            // 
            // button_import
            // 
            button_import.Location = new System.Drawing.Point(338, 62);
            button_import.Name = "button_import";
            button_import.Size = new System.Drawing.Size(90, 26);
            button_import.TabIndex = 2;
            button_import.Text = "Import";
            button_import.UseVisualStyleBackColor = true;
            // 
            // label_behavior
            // 
            label_behavior.AutoSize = true;
            label_behavior.Location = new System.Drawing.Point(12, 101);
            label_behavior.Name = "label_behavior";
            label_behavior.Size = new System.Drawing.Size(80, 15);
            label_behavior.TabIndex = 3;
            label_behavior.Text = "After import:";
            // 
            // comboBox_behavior
            // 
            comboBox_behavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox_behavior.Items.AddRange(new object[] { "Ask every time", "Download directly", "Do nothing" });
            comboBox_behavior.Location = new System.Drawing.Point(104, 97);
            comboBox_behavior.Name = "comboBox_behavior";
            comboBox_behavior.Size = new System.Drawing.Size(200, 23);
            comboBox_behavior.TabIndex = 4;
            // 
            // textBox_summary
            // 
            textBox_summary.Location = new System.Drawing.Point(12, 128);
            textBox_summary.Multiline = true;
            textBox_summary.Name = "textBox_summary";
            textBox_summary.ReadOnly = true;
            textBox_summary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBox_summary.Size = new System.Drawing.Size(416, 100);
            textBox_summary.TabIndex = 5;
            // 
            // button_close
            // 
            button_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            button_close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button_close.Location = new System.Drawing.Point(356, 240);
            button_close.Name = "button_close";
            button_close.Size = new System.Drawing.Size(72, 26);
            button_close.TabIndex = 6;
            button_close.Text = "Close";
            button_close.UseVisualStyleBackColor = true;
            // 
            // OsuCollectorImportForm
            // 
            AcceptButton = button_import;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = button_close;
            ClientSize = new System.Drawing.Size(440, 278);
            Controls.Add(button_close);
            Controls.Add(textBox_summary);
            Controls.Add(comboBox_behavior);
            Controls.Add(label_behavior);
            Controls.Add(button_import);
            Controls.Add(textBox_link);
            Controls.Add(label_linkInfo);
            Name = "OsuCollectorImportForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Import collection from osu!collector";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label_linkInfo;
        private System.Windows.Forms.TextBox textBox_link;
        private System.Windows.Forms.Button button_import;
        private System.Windows.Forms.Label label_behavior;
        private System.Windows.Forms.ComboBox comboBox_behavior;
        private System.Windows.Forms.TextBox textBox_summary;
        private System.Windows.Forms.Button button_close;
    }
}