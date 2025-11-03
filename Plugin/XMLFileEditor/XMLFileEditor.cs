using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ScintillaNET;

namespace MapCreator.Engine.Plugin.XMLFileEditor
{
    public partial class xmlFileEditor : Form
    {
        private string _filePath;
        private bool _isDirty = false;
        private bool _loading = false;
        private const int QUOTE_INDICATOR = 0;
        private readonly Color quoteColor = Color.LightSkyBlue;

        public xmlFileEditor(string xmlPath)
        {
            InitializeComponent();

            _filePath = xmlPath;
            ConfigureScintilla();
            WireUpEvents();
            LoadXml();
            xmlFileEditor_scintillaDisplay.Focus();
        }

        private void WireUpEvents()
        {
            xmlFileEditor_scintillaDisplay.TextChanged += xmlFileEditor_scintillaDisplay_TextChanged;        
        }

        private void LoadXml()
        {
            try
            {
                _loading = true;
                if (File.Exists(_filePath))
                    xmlFileEditor_scintillaDisplay.Text = File.ReadAllText(_filePath);
                else
                    xmlFileEditor_scintillaDisplay.Text = "";
                _isDirty = false;
                UpdateFormTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _loading = false;
                HighlightQuotes();
            }
        }

        private void xmlFileEditor_menuStrip_menuStripButton_saveFile_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(_filePath, xmlFileEditor_scintillaDisplay.Text);
                _isDirty = false;
                UpdateFormTitle();
                MessageBox.Show("File saved.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void xmlFileEditor_menuStrip_menuStripButton_loadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog.Title = "Open XML File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Update the current file path
                        _filePath = openFileDialog.FileName;
                        // Load the XML file contents into the Scintilla control
                        xmlFileEditor_scintillaDisplay.Text = File.ReadAllText(_filePath);
                        _isDirty = false;
                        UpdateFormTitle();
                        HighlightQuotes();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to load file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ConfigureScintilla()
        {
            var scintilla = xmlFileEditor_scintillaDisplay;

            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 12;
            scintilla.Styles[Style.Default].BackColor = Color.FromArgb(30, 30, 30);
            scintilla.Styles[Style.Default].ForeColor = Color.Aqua;

            // Add extra line spacing (4 px above and below)
            scintilla.DirectMessage(2523, new IntPtr(4)); // SCI_SETEXTRAASCENT
            scintilla.DirectMessage(2525, new IntPtr(4)); // SCI_SETEXTRADESCENT

            scintilla.StyleClearAll();

            // Set line number color (AFTER StyleClearAll)
            scintilla.Styles[Style.LineNumber].ForeColor = Color.Red;

            // Margin 0: Line numbers
            scintilla.Margins[0].Type = MarginType.Number;
            scintilla.Margins[0].Width = 35;
            scintilla.Margins[0].BackColor = Color.Black;

            // Margin 2: Folding
            Color marginBack = Color.FromArgb(45, 45, 48);
            scintilla.Margins[2].Type = MarginType.Symbol;
            scintilla.Margins[2].Width = 20;
            scintilla.Margins[2].BackColor = marginBack;

            // Hide unused margins
            for (int i = 1; i < scintilla.Margins.Count; i++)
            {
                if (i != 2)
                {
                    scintilla.Margins[i].Width = 0;
                    scintilla.Margins[i].Type = MarginType.Symbol;
                    scintilla.Margins[i].BackColor = marginBack;
                }
            }

            // VS Code Dark Syntax Highlighting for XML
            Color orange = Color.FromArgb(206, 145, 120);
            scintilla.Styles[Style.Xml.Default].ForeColor = orange;
            scintilla.Styles[Style.Xml.Other].ForeColor = orange;
            scintilla.Styles[Style.Xml.Entity].ForeColor = orange;
            scintilla.Styles[Style.Xml.Tag].ForeColor = Color.FromArgb(86, 156, 214);
            scintilla.Styles[Style.Xml.TagEnd].ForeColor = Color.FromArgb(86, 156, 214);
            scintilla.Styles[Style.Xml.Attribute].ForeColor = Color.FromArgb(156, 220, 254);
            scintilla.Styles[Style.Xml.Value].ForeColor = orange;
            scintilla.Styles[Style.Xml.Comment].ForeColor = Color.FromArgb(87, 166, 74);

            // Folding
            scintilla.SetProperty("fold", "1");
            scintilla.SetProperty("fold.compact", "1");
            scintilla.Margins[2].Mask = Marker.MaskFolders;
            scintilla.Margins[2].Sensitive = true;
            scintilla.SetFoldMarginColor(true, marginBack);
            scintilla.SetFoldMarginHighlightColor(true, Color.FromArgb(30, 30, 30));

            // Caret and selection
            scintilla.CaretForeColor = Color.White;
            scintilla.SetSelectionBackColor(true, Color.FromArgb(139, 0, 0));
            scintilla.CaretLineVisible = true;
            scintilla.CaretLineBackColor = Color.FromArgb(40, 40, 40);

            // --- Quote Indicator Setup ---
            scintilla.Indicators[QUOTE_INDICATOR].Style = IndicatorStyle.TextFore;
            scintilla.Indicators[QUOTE_INDICATOR].ForeColor = quoteColor;

            // Set lexer for XML
            scintilla.LexerName = "xml";
        }

        private void xmlFileEditor_scintillaDisplay_TextChanged(object sender, EventArgs e)
        {
            if (!_loading)
            {
                _isDirty = true;
                UpdateFormTitle();
            }
            HighlightQuotes();
        }

        private void HighlightQuotes()
        {
            var scintilla = xmlFileEditor_scintillaDisplay;
            scintilla.IndicatorClearRange(0, scintilla.TextLength);

            int pos = 0;
            while (pos < scintilla.TextLength)
            {
                if (scintilla.GetCharAt(pos) == '"')
                {
                    scintilla.IndicatorCurrent = QUOTE_INDICATOR;
                    scintilla.IndicatorFillRange(pos, 1);
                }
                pos++;
            }
        }

        private void UpdateFormTitle()
        {
            string dirtyMark = _isDirty ? "*" : "";
            this.Text = $"{dirtyMark}Editing: {Path.GetFileName(_filePath)}";
        }

        private void xmlFileEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDirty)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Save before closing?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == DialogResult.Yes)
                {
                    xmlFileEditor_menuStrip_menuStripButton_saveFile_Click(this, EventArgs.Empty);
                }
            }
        }

        // Process Ctrl+S shortcut anywhere in the form
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                xmlFileEditor_menuStrip_menuStripButton_saveFile_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
