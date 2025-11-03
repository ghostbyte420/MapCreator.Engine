namespace MapCreator.Engine.Plugin.XMLFileEditor
{
    partial class xmlFileEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(xmlFileEditor));
            xmlFileEditor_scintillaDisplay = new ScintillaNET.Scintilla();
            xmlFileEditor_menuStrip = new MenuStrip();
            xmlFileEditor_menuStrip_menuStripButton_loadFile = new ToolStripMenuItem();
            xmlFileEditor_menuStrip_menuStripButton_saveFile = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            xmlFileEditor_menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // xmlFileEditor_scintillaDisplay
            // 
            xmlFileEditor_scintillaDisplay.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 215);
            xmlFileEditor_scintillaDisplay.BorderStyle = ScintillaNET.BorderStyle.FixedSingle;
            xmlFileEditor_scintillaDisplay.Dock = DockStyle.Fill;
            xmlFileEditor_scintillaDisplay.LexerName = null;
            xmlFileEditor_scintillaDisplay.Location = new Point(0, 33);
            xmlFileEditor_scintillaDisplay.Name = "xmlFileEditor_scintillaDisplay";
            xmlFileEditor_scintillaDisplay.ScrollWidth = 49;
            xmlFileEditor_scintillaDisplay.Size = new Size(1087, 521);
            xmlFileEditor_scintillaDisplay.TabIndex = 1;
            xmlFileEditor_scintillaDisplay.Text = "scintilla1";
            xmlFileEditor_scintillaDisplay.TextChanged += xmlFileEditor_scintillaDisplay_TextChanged;
            // 
            // xmlFileEditor_menuStrip
            // 
            xmlFileEditor_menuStrip.BackgroundImage = (Image)resources.GetObject("xmlFileEditor_menuStrip.BackgroundImage");
            xmlFileEditor_menuStrip.BackgroundImageLayout = ImageLayout.Stretch;
            xmlFileEditor_menuStrip.Font = new Font("Segoe UI", 11F);
            xmlFileEditor_menuStrip.ImageScalingSize = new Size(25, 25);
            xmlFileEditor_menuStrip.Items.AddRange(new ToolStripItem[] { xmlFileEditor_menuStrip_menuStripButton_loadFile, xmlFileEditor_menuStrip_menuStripButton_saveFile, toolStripMenuItem1 });
            xmlFileEditor_menuStrip.Location = new Point(0, 0);
            xmlFileEditor_menuStrip.Name = "xmlFileEditor_menuStrip";
            xmlFileEditor_menuStrip.Size = new Size(1087, 33);
            xmlFileEditor_menuStrip.TabIndex = 2;
            xmlFileEditor_menuStrip.Text = "menuStrip2";
            // 
            // xmlFileEditor_menuStrip_menuStripButton_loadFile
            // 
            xmlFileEditor_menuStrip_menuStripButton_loadFile.Image = (Image)resources.GetObject("xmlFileEditor_menuStrip_menuStripButton_loadFile.Image");
            xmlFileEditor_menuStrip_menuStripButton_loadFile.Margin = new Padding(975, 0, 10, 0);
            xmlFileEditor_menuStrip_menuStripButton_loadFile.Name = "xmlFileEditor_menuStrip_menuStripButton_loadFile";
            xmlFileEditor_menuStrip_menuStripButton_loadFile.Size = new Size(37, 29);
            xmlFileEditor_menuStrip_menuStripButton_loadFile.Click += xmlFileEditor_menuStrip_menuStripButton_loadFile_Click;
            // 
            // xmlFileEditor_menuStrip_menuStripButton_saveFile
            // 
            xmlFileEditor_menuStrip_menuStripButton_saveFile.Image = (Image)resources.GetObject("xmlFileEditor_menuStrip_menuStripButton_saveFile.Image");
            xmlFileEditor_menuStrip_menuStripButton_saveFile.Name = "xmlFileEditor_menuStrip_menuStripButton_saveFile";
            xmlFileEditor_menuStrip_menuStripButton_saveFile.Size = new Size(37, 29);
            xmlFileEditor_menuStrip_menuStripButton_saveFile.Click += xmlFileEditor_menuStrip_menuStripButton_saveFile_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(12, 29);
            // 
            // xmlFileEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1087, 554);
            Controls.Add(xmlFileEditor_scintillaDisplay);
            Controls.Add(xmlFileEditor_menuStrip);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "xmlFileEditor";
            Text = "XMLFileEditor";
            FormClosing += xmlFileEditor_FormClosing;
            xmlFileEditor_menuStrip.ResumeLayout(false);
            xmlFileEditor_menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ScintillaNET.Scintilla xmlFileEditor_scintillaDisplay;
        private MenuStrip xmlFileEditor_menuStrip;
        private ToolStripMenuItem xmlFileEditor_menuStrip_menuStripButton_saveFile;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem xmlFileEditor_menuStrip_menuStripButton_loadFile;
    }
}