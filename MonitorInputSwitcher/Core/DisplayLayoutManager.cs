using MonitorInputSwitcher.Models;

namespace MonitorInputSwitcher.Core
{
    /// <summary>
    /// Manages display layout visualization and grid positioning
    /// </summary>
    public class DisplayLayoutManager
    {
        private readonly int _gridCols;
        private readonly int _gridRows;
        private readonly Size _cellSize;
        private readonly Color[] _colorPalette;
        
        private readonly Dictionary<string, Rectangle> _layoutRects = new();
        private readonly Dictionary<string, Color> _deviceColors = new();
        private readonly Dictionary<string, string> _deviceNames = new();

        public DisplayLayoutManager(int gridCols, int gridRows, Color[] colorPalette)
        {
            _gridCols = gridCols;
            _gridRows = gridRows;
            _colorPalette = colorPalette;
            _cellSize = new Size((660 - 20) / gridCols, (240 - 20) / gridRows);
        }

        /// <summary>
        /// Updates the layout with current monitor configuration
        /// </summary>
        public void UpdateLayout(List<DisplayMonitor> monitors)
        {
            _layoutRects.Clear();
            _deviceColors.Clear();
            _deviceNames.Clear();

            if (monitors.Count == 0) return;

            // Calculate virtual desktop bounds
            int minX = monitors.Min(m => m.Bounds.X);
            int minY = monitors.Min(m => m.Bounds.Y);
            int maxX = monitors.Max(m => m.Bounds.Right);
            int maxY = monitors.Max(m => m.Bounds.Bottom);
            
            int virtualW = Math.Max(1, maxX - minX);
            int virtualH = Math.Max(1, maxY - minY);

            for (int i = 0; i < monitors.Count; i++)
            {
                var monitor = monitors[i];
                
                // Assign color
                var color = _colorPalette[i % _colorPalette.Length];
                _deviceColors[monitor.DeviceName] = color;
                _deviceNames[monitor.DeviceName] = monitor.FriendlyName;

                // Calculate grid position
                int centerX = monitor.Bounds.X + monitor.Bounds.Width / 2;
                int centerY = monitor.Bounds.Y + monitor.Bounds.Height / 2;
                
                double normX = (centerX - minX) / (double)virtualW;
                double normY = (centerY - minY) / (double)virtualH;
                
                int cellX = (int)Math.Round(normX * (_gridCols - 1));
                int cellY = (int)Math.Round(normY * (_gridRows - 1));
                
                cellX = Math.Max(0, Math.Min(_gridCols - 1, cellX));
                cellY = Math.Max(0, Math.Min(_gridRows - 1, cellY));

                // Calculate cell rectangle
                int x = 10 + cellX * _cellSize.Width + 4;
                int y = 10 + cellY * _cellSize.Height + 4;
                int w = _cellSize.Width - 8;
                int h = _cellSize.Height - 8;
                
                _layoutRects[monitor.DeviceName] = new Rectangle(x, y, w, h);
            }
        }

        /// <summary>
        /// Paints the layout grid and monitors
        /// </summary>
        public void PaintLayout(Graphics graphics, Size panelSize)
        {
            graphics.Clear(SystemColors.Control);
            
            // Draw grid lines
            using var gridPen = new Pen(Color.FromArgb(100, 100, 100), 1f);
            
            for (int c = 0; c <= _gridCols; c++)
            {
                int x = 10 + c * _cellSize.Width;
                graphics.DrawLine(gridPen, x, 10, x, 10 + _gridRows * _cellSize.Height);
            }
            
            for (int r = 0; r <= _gridRows; r++)
            {
                int y = 10 + r * _cellSize.Height;
                graphics.DrawLine(gridPen, 10, y, 10 + _gridCols * _cellSize.Width, y);
            }

            // Draw monitors
            using var font = new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Bold);
            using var pen = new Pen(Color.Black, 2f);
            
            foreach (var kvp in _layoutRects)
            {
                var rect = kvp.Value;
                var deviceName = kvp.Key;
                
                // Fill background
                var backColor = _deviceColors.GetValueOrDefault(deviceName, Color.LightGray);
                using var backBrush = new SolidBrush(backColor);
                graphics.FillRectangle(backBrush, rect);
                graphics.DrawRectangle(pen, rect);
                
                // Draw text
                var displayName = _deviceNames.GetValueOrDefault(deviceName, deviceName);
                var formattedText = FormatDeviceText(displayName);
                
                var textRect = new RectangleF(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.Word,
                    FormatFlags = StringFormatFlags.NoClip
                };
                
                using var textBrush = new SolidBrush(Color.Black);
                graphics.DrawString(formattedText, font, textBrush, textRect, format);
            }
        }

        /// <summary>
        /// Gets the color assigned to a monitor
        /// </summary>
        public Color GetMonitorColor(string deviceName)
        {
            return _deviceColors.GetValueOrDefault(deviceName, Color.Red);
        }

        private string FormatDeviceText(string deviceText)
        {
            const int maxLength = 70;
            
            if (deviceText.Length <= maxLength)
                return deviceText;
                
            var idxMatch = System.Text.RegularExpressions.Regex.Match(deviceText, @"\((\d+)\)$");
            if (!idxMatch.Success)
                return deviceText.Substring(0, maxLength) + "...";
                
            string idxPart = idxMatch.Value;
            string namePart = deviceText.Substring(0, deviceText.Length - idxPart.Length).Trim();
            
            if (namePart.Length <= maxLength)
                return $"{namePart} {idxPart}";
            else
                return $"{namePart.Substring(0, maxLength)}... {idxPart}";
        }
    }
}