using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace MapCreator.Engine.Plugin.FacetDesigner
{
    public partial class facetDesigner : Form
    {
        private List<TerrainInfo> terrainList = new();
        private List<AltitudeInfo> altitudeList = new();
        private Color[] currentPalette = null;
        private enum PaletteMode { None, Terrain, Altitude }
        private PaletteMode currentPaletteMode = PaletteMode.None;

        private Bitmap currentImage = null;
        private float currentZoom = 1.0f;
        private readonly float minZoom = 0.13f;
        private readonly float maxZoom = 200.0f;
        private Point? highlightedTile = null;

        private bool isPanning = false;
        private Point panStartPoint;
        private Point scrollStartPoint;
        private Point pictureBoxStartLocation;

        public facetDesigner()
        {
            InitializeComponent();

            facetDesigner_panel_pictureBox_facetCanvas.BackColor = Color.Silver;
            SetDoubleBuffered(facetDesigner_panel_pictureBox_facetCanvas);

            // numeric zoom settings
            facetDesigner_toolStrip_numericUpDown_zoomLevel.Minimum = (decimal)minZoom;
            facetDesigner_toolStrip_numericUpDown_zoomLevel.Maximum = (decimal)maxZoom;
            facetDesigner_toolStrip_numericUpDown_zoomLevel.DecimalPlaces = 2;
            facetDesigner_toolStrip_numericUpDown_zoomLevel.Increment = 0.1M;
            facetDesigner_toolStrip_numericUpDown_zoomLevel.Value = (decimal)currentZoom;
            facetDesigner_toolStrip_numericUpDown_zoomLevel.ValueChanged += FacetDesigner_toolStrip_numericUpDown_zoomLevel_ValueChanged;

            // menu / toolbar wiring
            facetDesigner_menuStrip_menuStripButton_selectColorPalette_loadTerrain.Click += facetDesigner_menuStrip_menuStripButton_selectColorPalette_loadTerrain_Click;
            facetDesigner_menuStrip_menuStripButton_selectColorPalette_loadAltitude.Click += facetDesigner_menuStrip_menuStripButton_selectColorPalette_loadAltitude_Click;
            facetDesigner_menuStrip_menuStripButton_selectFacetBitmap_importTerrainBitmap.Click += facetDesigner_menuStrip_menuStripButton_selectFacetBitmap_importTerrainBitmap_Click;
            facetDesigner_menuStrip_menuStripButton_selectFacetBitmap_importAltitudeBitmap.Click += facetDesigner_menuStrip_menuStripButton_selectFacetBitmap_importAltitudeBitmap_Click;

            // canvas event hookups
            facetDesigner_panel_pictureBox_facetCanvas.Paint += facetDesigner_panel_pictureBox_facetCanvas_Paint;
            facetDesigner_panel_pictureBox_facetCanvas.MouseWheel += FacetCanvas_MouseWheel;
            facetDesigner_panel_pictureBox_facetCanvas.MouseEnter += (s, e) => facetDesigner_panel_pictureBox_facetCanvas.Focus();
            facetDesigner_panel_pictureBox_facetCanvas.MouseMove += FacetCanvas_MouseDrag;
            facetDesigner_panel_pictureBox_facetCanvas.MouseLeave += (s, e) =>
            {
                highlightedTile = null;
                facetDesigner_panel_pictureBox_facetCanvas.Invalidate();
            };
            facetDesigner_panel_pictureBox_facetCanvas.MouseDown += FacetCanvas_MouseDown;
            facetDesigner_panel_pictureBox_facetCanvas.MouseUp += FacetCanvas_MouseUp;

            facetDesigner_panel_pictureBox_facetCanvas.Dock = DockStyle.None;
            facetDesigner_panel_pictureBox_facetCanvas.SizeMode = PictureBoxSizeMode.Normal;
        }

        public static void SetDoubleBuffered(Control c)
        {
            if (SystemInformation.TerminalServerSession) return;
            var aProp = typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }

        // ---- Zoom Synchronization (prevents double update) ----
        private bool _suppressZoomEvent = false;
        private void SetZoomLevelWithoutEvent(float zoom)
        {
            zoom = Math.Max(minZoom, Math.Min(maxZoom, zoom));
            try
            {
                _suppressZoomEvent = true;
                facetDesigner_toolStrip_numericUpDown_zoomLevel.Value =
                    (decimal)zoom;
                currentZoom = zoom;
                UpdateCanvasSizeAndInvalidate();
            }
            finally
            {
                _suppressZoomEvent = false;
            }
        }

        private void FacetDesigner_toolStrip_numericUpDown_zoomLevel_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressZoomEvent) return;
            var v = (float)facetDesigner_toolStrip_numericUpDown_zoomLevel.Value;
            currentZoom = Math.Max(minZoom, Math.Min(maxZoom, v));
            UpdateCanvasSizeAndInvalidate();
        }

        // ---- Palette Loading (uses FlowLayoutPanel for swatches) ----
        private void facetDesigner_menuStrip_menuStripButton_selectColorPalette_loadTerrain_Click(object sender, EventArgs e)
        {
            string xmlPath = System.IO.Path.Combine(Application.StartupPath, "MapCompiler", "Engine", "Terrain.xml");
            if (!System.IO.File.Exists(xmlPath))
            {
                MessageBox.Show($"File not found:\n{xmlPath}", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            terrainList = LoadTerrainFromXML(xmlPath);

            FlowLayoutPanel swatchPanel = facetDesigner_panel_swatchColors as FlowLayoutPanel;
            if (swatchPanel != null)
            {
                swatchPanel.Controls.Clear();
                foreach (var item in terrainList)
                {
                    Button swatch = new()
                    {
                        BackColor = item.Color,
                        Width = 32,
                        Height = 32,
                        Tag = item.ID,
                        Margin = new Padding(3)
                    };
                    swatch.Click += TerrainSwatch_Click;
                    new ToolTip().SetToolTip(swatch, item.Name);
                    swatchPanel.Controls.Add(swatch);
                }
            }
            else
            {
                facetDesigner_panel_swatchColors.Controls.Clear();
                int x = 10, y = 10, buttonSize = 32, margin = 5;
                for (int i = terrainList.Count - 1; i >= 0; i--)
                {
                    var item = terrainList[i];
                    Button swatch = new()
                    {
                        BackColor = item.Color,
                        Width = buttonSize,
                        Height = buttonSize,
                        Left = x,
                        Top = y,
                        Tag = item.ID
                    };
                    swatch.Click += TerrainSwatch_Click;
                    new ToolTip().SetToolTip(swatch, item.Name);
                    facetDesigner_panel_swatchColors.Controls.Add(swatch);

                    x += buttonSize + margin;
                    if (x + buttonSize > facetDesigner_panel_swatchColors.Width)
                    {
                        x = 10;
                        y += buttonSize + margin;
                    }
                }
            }
            currentPalette = GetTerrainPalette();
            currentPaletteMode = PaletteMode.Terrain;
        }

        private void facetDesigner_menuStrip_menuStripButton_selectColorPalette_loadAltitude_Click(object sender, EventArgs e)
        {
            string xmlPath = System.IO.Path.Combine(Application.StartupPath, "MapCompiler", "Engine", "Altitude.xml");
            if (!System.IO.File.Exists(xmlPath))
            {
                MessageBox.Show($"File not found:\n{xmlPath}", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            altitudeList = LoadAltitudeFromXML(xmlPath);

            FlowLayoutPanel swatchPanel = facetDesigner_panel_swatchColors as FlowLayoutPanel;
            if (swatchPanel != null)
            {
                swatchPanel.Controls.Clear();
                foreach (var item in altitudeList)
                {
                    Button swatch = new()
                    {
                        BackColor = item.Color,
                        Width = 32,
                        Height = 32,
                        Tag = item.Key,
                        Margin = new Padding(3)
                    };
                    swatch.Click += AltitudeSwatch_Click;
                    new ToolTip().SetToolTip(swatch, $"Key: {item.Key}, Type: {item.Type}, Altitude: {item.Altitude}");
                    swatchPanel.Controls.Add(swatch);
                }
            }
            else
            {
                facetDesigner_panel_swatchColors.Controls.Clear();
                int x = 10, y = 10, buttonSize = 32, margin = 5;
                for (int i = altitudeList.Count - 1; i >= 0; i--)
                {
                    var item = altitudeList[i];
                    Button swatch = new()
                    {
                        BackColor = item.Color,
                        Width = buttonSize,
                        Height = buttonSize,
                        Left = x,
                        Top = y,
                        Tag = item.Key
                    };
                    swatch.Click += AltitudeSwatch_Click;
                    new ToolTip().SetToolTip(swatch, $"Key: {item.Key}, Type: {item.Type}, Altitude: {item.Altitude}");
                    facetDesigner_panel_swatchColors.Controls.Add(swatch);

                    x += buttonSize + margin;
                    if (x + buttonSize > facetDesigner_panel_swatchColors.Width)
                    {
                        x = 10;
                        y += buttonSize + margin;
                    }
                }
            }
            currentPalette = GetAltitudePalette();
            currentPaletteMode = PaletteMode.Altitude;
        }

        private Color[] GetTerrainPalette()
        {
            Color[] palette = new Color[terrainList.Count];
            for (int i = 0; i < terrainList.Count; i++) palette[i] = terrainList[i].Color;
            return palette;
        }

        private Color[] GetAltitudePalette()
        {
            Color[] palette = new Color[altitudeList.Count];
            for (int i = 0; i < altitudeList.Count; i++) palette[i] = altitudeList[i].Color;
            return palette;
        }

        // ---- Bitmap Import ----
        private void facetDesigner_menuStrip_menuStripButton_selectFacetBitmap_importTerrainBitmap_Click(object sender, EventArgs e)
        {
            if (currentPaletteMode != PaletteMode.Terrain || currentPalette == null)
            {
                MessageBox.Show("Load the Terrain swatch first!", "Palette Needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ImportIndexedBitmap("Terrain.bmp", currentPalette);
        }

        private void facetDesigner_menuStrip_menuStripButton_selectFacetBitmap_importAltitudeBitmap_Click(object sender, EventArgs e)
        {
            if (currentPaletteMode != PaletteMode.Altitude || currentPalette == null)
            {
                MessageBox.Show("Load the Altitude swatch first!", "Palette Needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ImportIndexedBitmap("Altitude.bmp", currentPalette);
        }

        private void ImportIndexedBitmap(string defaultFilename, Color[] palette)
        {
            using OpenFileDialog dlg = new()
            {
                Title = $"Select {defaultFilename}",
                Filter = "Bitmap files (*.bmp)|*.bmp",
                FileName = defaultFilename
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using var bmp = new Bitmap(dlg.FileName);
                if (bmp.PixelFormat != PixelFormat.Format8bppIndexed)
                {
                    MessageBox.Show("Only 8-bit indexed BMP files are supported.", "Wrong Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Bitmap displayBmp = ApplyPaletteToIndexedBitmap(bmp, palette);
                SetImageAndZoom(displayBmp);
                GC.Collect(); // Optional: help with memory if loading many large images
            }
        }

        private unsafe Bitmap ApplyPaletteToIndexedBitmap(Bitmap bmp, Color[] xmlPalette)
        {
            ColorPalette bmpPalette = bmp.Palette;
            int width = bmp.Width;
            int height = bmp.Height;
            Bitmap outBmp = new(width, height, PixelFormat.Format24bppRgb);
            var srcData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, bmp.PixelFormat);
            var dstData = outBmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            // Build a lookup for XML palette colors
            var xmlColorLookup = new Dictionary<int, Color>();
            foreach (var c in xmlPalette) xmlColorLookup[c.ToArgb()] = c;

            try
            {
                int srcStride = srcData.Stride;
                int dstStride = dstData.Stride;
                byte* srcBase = (byte*)srcData.Scan0;
                byte* dstBase = (byte*)dstData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* srcPtr = srcBase + y * srcStride;
                    byte* dstPtr = dstBase + y * dstStride;
                    for (int x = 0; x < width; x++)
                    {
                        byte idx = srcPtr[x];
                        Color bmpColor = bmpPalette.Entries[idx];
                        Color xmlColor = xmlColorLookup.TryGetValue(bmpColor.ToArgb(), out var found) ? found : Color.Magenta;
                        dstPtr[x * 3 + 0] = xmlColor.B;
                        dstPtr[x * 3 + 1] = xmlColor.G;
                        dstPtr[x * 3 + 2] = xmlColor.R;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(srcData);
                outBmp.UnlockBits(dstData);
            }
            return outBmp;
        }

        private void SetImageAndZoom(Bitmap bmp)
        {
            currentImage?.Dispose();
            currentImage = bmp;
            ZoomToFit();
            facetDesigner_panel_facetCanvas.AutoScrollPosition = new Point(0, 0);
        }

        // ---- Zoom & Canvas Updates ----
        private void ZoomToFit()
        {
            if (currentImage == null) return;
            var panelSize = facetDesigner_panel_facetCanvas.ClientSize;
            float zoomX = (float)panelSize.Width / currentImage.Width;
            float zoomY = (float)panelSize.Height / currentImage.Height;
            float fitZoom = Math.Min(zoomX, zoomY);
            SetZoomLevelWithoutEvent(fitZoom);
        }

        private void ZoomTo100() { SetZoomLevelWithoutEvent(1.0f); }
        private void ZoomTo50() { SetZoomLevelWithoutEvent(0.5f); }

        private void UpdateCanvasSizeAndInvalidate()
        {
            if (currentImage != null)
            {
                // Get center point before zoom (in image coordinates)
                Point currentLoc = facetDesigner_panel_pictureBox_facetCanvas.Location;
                Size panelSize = facetDesigner_panel_facetCanvas.ClientSize;

                // Calculate the center of the visible area in image coordinates
                float centerX = (-currentLoc.X + panelSize.Width / 2f) / (currentImage.Width * currentZoom);
                float centerY = (-currentLoc.Y + panelSize.Height / 2f) / (currentImage.Height * currentZoom);

                int newWidth = (int)(currentImage.Width * currentZoom);
                int newHeight = (int)(currentImage.Height * currentZoom);
                facetDesigner_panel_pictureBox_facetCanvas.Size = new Size(newWidth, newHeight);

                facetDesigner_panel_pictureBox_facetCanvas.Location = new Point(
                    Math.Max(0, (facetDesigner_panel_facetCanvas.ClientSize.Width - newWidth) / 2),
                    Math.Max(0, (facetDesigner_panel_facetCanvas.ClientSize.Height - newHeight) / 2)
                );
            }
            facetDesigner_panel_pictureBox_facetCanvas.Invalidate();
        }

        private void FacetCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if (currentImage == null || ModifierKeys != Keys.Control) return;

            const float zoomStep = 1.10f;
            float newZoom = e.Delta > 0 ? Math.Min(maxZoom, currentZoom * zoomStep) : Math.Max(minZoom, currentZoom / zoomStep);
            SetZoomLevelWithoutEvent(newZoom);
        }

        // ---- Mouse Panning & Highlight ----
        private Point ClampScroll(Point scroll, Size imageSize)
        {
            int panelW = facetDesigner_panel_facetCanvas.ClientSize.Width;
            int panelH = facetDesigner_panel_facetCanvas.ClientSize.Height;
            int imgW = imageSize.Width;
            int imgH = imageSize.Height;
            int maxX = Math.Max(0, imgW - panelW);
            int maxY = Math.Max(0, imgH - panelH);
            return new Point(
                Math.Max(0, Math.Min(scroll.X, maxX)),
                Math.Max(0, Math.Min(scroll.Y, maxY))
            );
        }

        private void FacetCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right)
            {
                isPanning = true;
                panStartPoint = e.Location;
                pictureBoxStartLocation = facetDesigner_panel_pictureBox_facetCanvas.Location;
                facetDesigner_panel_pictureBox_facetCanvas.Cursor = Cursors.SizeAll;
            }
        }

        private void FacetCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right)
            {
                isPanning = false;
                facetDesigner_panel_pictureBox_facetCanvas.Cursor = Cursors.Default;
            }
        }

        private void FacetCanvas_MouseDrag(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                // Calculate movement delta
                int dx = e.X - panStartPoint.X;
                int dy = e.Y - panStartPoint.Y;

                // Calculate new position
                int newX = pictureBoxStartLocation.X + dx;
                int newY = pictureBoxStartLocation.Y + dy;

                // Apply clamping to keep image visible
                if (currentImage != null)
                {
                    int scaledWidth = (int)(currentImage.Width * currentZoom);
                    int scaledHeight = (int)(currentImage.Height * currentZoom);
                    int panelWidth = facetDesigner_panel_facetCanvas.ClientSize.Width;
                    int panelHeight = facetDesigner_panel_facetCanvas.ClientSize.Height;

                    // Clamp so at least part of the image is always visible
                    int minX = panelWidth - scaledWidth;
                    int maxX = 0;
                    int minY = panelHeight - scaledHeight;
                    int maxY = 0;

                    // If image is smaller than panel, allow centering
                    if (scaledWidth < panelWidth)
                    {
                        minX = 0;
                        maxX = panelWidth - scaledWidth;
                    }
                    if (scaledHeight < panelHeight)
                    {
                        minY = 0;
                        maxY = panelHeight - scaledHeight;
                    }

                    newX = Math.Max(minX, Math.Min(maxX, newX));
                    newY = Math.Max(minY, Math.Min(maxY, newY));
                }

                facetDesigner_panel_pictureBox_facetCanvas.Location = new Point(newX, newY);
            }
            else
            {
                // Update tile highlighting
                facetDesigner_panel_pictureBox_facetCanvas_MouseMove(sender, e);
            }
        }

        private void facetDesigner_panel_pictureBox_facetCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentImage == null) { highlightedTile = null; return; }
            int px = (int)Math.Floor(e.X / currentZoom);
            int py = (int)Math.Floor(e.Y / currentZoom);

            // Show coordinates in the form title
            this.Text = $"FacetDesigner - ({px}, {py})";

            if (px >= 0 && px < currentImage.Width && py >= 0 && py < currentImage.Height)
            {
                if (highlightedTile == null || highlightedTile.Value.X != px || highlightedTile.Value.Y != py)
                {
                    highlightedTile = new Point(px, py);
                    facetDesigner_panel_pictureBox_facetCanvas.Invalidate();
                }
            }
            else if (highlightedTile != null)
            {
                highlightedTile = null;
                facetDesigner_panel_pictureBox_facetCanvas.Invalidate();
            }
        }

        // ---- OPTIMIZED PAINT HANDLER ----
        private void facetDesigner_panel_pictureBox_facetCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (currentImage == null) return;
            float scale = currentZoom;
            int imgW = currentImage.Width;
            int imgH = currentImage.Height;

            // Calculate the visible source rectangle (viewport)
            Rectangle destRect = e.ClipRectangle;
            int srcX = (int)(destRect.X / scale);
            int srcY = (int)(destRect.Y / scale);
            int srcW = (int)Math.Ceiling(destRect.Width / scale);
            int srcH = (int)Math.Ceiling(destRect.Height / scale);
            if (srcX < 0) srcX = 0;
            if (srcY < 0) srcY = 0;
            if (srcX + srcW > imgW) srcW = imgW - srcX;
            if (srcY + srcH > imgH) srcH = imgH - srcY;
            if (srcW <= 0 || srcH <= 0) return;

            // Fill background
            e.Graphics.Clear(facetDesigner_panel_pictureBox_facetCanvas.BackColor);

            // Draw only the visible portion
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.SmoothingMode = SmoothingMode.None;

            e.Graphics.DrawImage(
                currentImage,
                destRect,                             // destination rect (on screen)
                new Rectangle(srcX, srcY, srcW, srcH), // source rect (from image)
                GraphicsUnit.Pixel
            );

            // Draw highlight (overlay)
            if (highlightedTile != null)
            {
                var (tx, ty) = (highlightedTile.Value.X, highlightedTile.Value.Y);
                var rect = new RectangleF(tx * scale, ty * scale, scale, scale);
                using var brush = new SolidBrush(Color.FromArgb(120, Color.Yellow));
                using var pen = new Pen(Color.Red, Math.Max(1f, scale / 8f));
                e.Graphics.FillRectangle(brush, rect);
                e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        // ---- Toolstrip Zoom Buttons ---
        private void facetDesigner_toolStrip_toolStripButton_zoom100_Click(object sender, EventArgs e) => ZoomTo100();
        private void facetDesigner_toolStrip_toolStripButton_zoomFit_Click(object sender, EventArgs e) => ZoomToFit();
        private void facetDesigner_toolStrip_toolStripButton_zoom50_Click(object sender, EventArgs e) => ZoomTo50();

        // ---- XML Loaders ----
        public List<TerrainInfo> LoadTerrainFromXML(string xmlPath)
        {
            var terrains = new List<TerrainInfo>();
            var doc = XDocument.Load(xmlPath);
            foreach (var elem in doc.Descendants("Terrain"))
            {
                terrains.Add(new TerrainInfo
                {
                    Name = elem.Attribute("Name")?.Value,
                    ID = int.Parse(elem.Attribute("ID")?.Value ?? "0"),
                    TileID = int.Parse(elem.Attribute("TileID")?.Value ?? "0"),
                    Color = Color.FromArgb(
                        int.Parse(elem.Attribute("R")?.Value ?? "0"),
                        int.Parse(elem.Attribute("G")?.Value ?? "0"),
                        int.Parse(elem.Attribute("B")?.Value ?? "0")),
                    Base = int.Parse(elem.Attribute("Base")?.Value ?? "0"),
                    Random = bool.Parse(elem.Attribute("Random")?.Value ?? "False")
                });
            }
            return terrains;
        }

        public List<AltitudeInfo> LoadAltitudeFromXML(string xmlPath)
        {
            var altitudes = new List<AltitudeInfo>();
            var doc = XDocument.Load(xmlPath);
            foreach (var elem in doc.Descendants("Altitude"))
            {
                altitudes.Add(new AltitudeInfo
                {
                    Key = int.Parse(elem.Attribute("Key")?.Value ?? "0"),
                    Type = elem.Attribute("Type")?.Value,
                    Altitude = int.Parse(elem.Attribute("Altitude")?.Value ?? "0"),
                    Color = Color.FromArgb(
                        int.Parse(elem.Attribute("R")?.Value ?? "0"),
                        int.Parse(elem.Attribute("G")?.Value ?? "0"),
                        int.Parse(elem.Attribute("B")?.Value ?? "0"))
                });
            }
            return altitudes;
        }

        private void TerrainSwatch_Click(object sender, EventArgs e)
        {
            if (sender is Button btn)
                MessageBox.Show("Selected terrain color index: " + btn.Tag);
        }

        private void AltitudeSwatch_Click(object sender, EventArgs e)
        {
            if (sender is Button btn)
                MessageBox.Show("Selected altitude key: " + btn.Tag);
        }

        private void facetDesigner_menuStrip_menuStripButton_saveFacetBitmap_Click(object sender, EventArgs e)
        {

        }
    }

    // ---- Data Classes ----
    public class TerrainInfo
    {
        public string Name;
        public int ID;
        public int TileID;
        public Color Color;
        public int Base;
        public bool Random;
    }

    public class AltitudeInfo
    {
        public int Key;
        public string Type;
        public int Altitude;
        public Color Color;
    }
}