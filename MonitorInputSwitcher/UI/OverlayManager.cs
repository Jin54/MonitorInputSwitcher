namespace MonitorInputSwitcher.UI
{
    /// <summary>
    /// Manages overlay windows for monitor identification
    /// </summary>
    public class OverlayManager : IDisposable
    {
        private readonly Dictionary<string, MonitorOverlay> _activeOverlays = new();

        public bool HasActiveOverlays => _activeOverlays.Count > 0;

        /// <summary>
        /// Shows an overlay on the specified monitor
        /// </summary>
        public void ShowOverlay(string text, Rectangle bounds, Color textColor)
        {
            var overlay = new MonitorOverlay(text, bounds, textColor);
            overlay.Show();
            _activeOverlays[text] = overlay;
        }

        /// <summary>
        /// Hides all active overlays
        /// </summary>
        public void HideAllOverlays()
        {
            foreach (var overlay in _activeOverlays.Values.ToList())
            {
                try
                {
                    overlay.Close();
                }
                catch
                {
                    // Ignore errors when closing overlays
                }
            }
            _activeOverlays.Clear();
        }

        public void Dispose()
        {
            HideAllOverlays();
        }
    }

    /// <summary>
    /// Overlay form that displays monitor identification text
    /// </summary>
    public class MonitorOverlay : Form
    {
        private readonly string _text;
        private readonly Color _textColor;

        public MonitorOverlay(string text, Rectangle bounds, Color textColor)
        {
            _text = text;
            _textColor = textColor;
            
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Location = bounds.Location;
            Size = bounds.Size;
            BackColor = Color.Magenta;
            TransparencyKey = BackColor;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            
            Click += (s, e) => Close();
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000080 | 0x00000020; // WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Calculate maximum text width (1/5 of monitor width)
            float maxTextWidth = Width / 5f;
            
            // Find optimal font size
            float fontSize = Math.Min(48f, Height / 8f);
            Font? font = null;
            SizeF textSize;
            
            do
            {
                font?.Dispose();
                font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);
                
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.Word,
                    FormatFlags = StringFormatFlags.NoClip
                };
                
                textSize = e.Graphics.MeasureString(_text, font, (int)maxTextWidth, format);
                fontSize -= 2f;
                
            } while (textSize.Width > maxTextWidth && fontSize > 16f);
            
            // Ensure minimum readable size
            if (fontSize < 16f)
            {
                font?.Dispose();
                font = new Font(FontFamily.GenericSansSerif, 16f, FontStyle.Bold);
                textSize = e.Graphics.MeasureString(_text, font, (int)maxTextWidth);
            }
            
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            
            // Center the text
            var textRect = new RectangleF(
                (Width - maxTextWidth) / 2f,
                (Height - textSize.Height) / 2f,
                maxTextWidth,
                textSize.Height
            );
            
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.Word,
                FormatFlags = StringFormatFlags.NoClip
            };
            
            // Draw outline
            using var outlineBrush = new SolidBrush(Color.Black);
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    if (dx * dx + dy * dy <= 4 && (dx != 0 || dy != 0))
                    {
                        var outlineRect = new RectangleF(
                            textRect.X + dx, 
                            textRect.Y + dy, 
                            textRect.Width, 
                            textRect.Height
                        );
                        e.Graphics.DrawString(_text, font, outlineBrush, outlineRect, stringFormat);
                    }
                }
            }
            
            // Draw main text
            using var textBrush = new SolidBrush(_textColor);
            e.Graphics.DrawString(_text, font, textBrush, textRect, stringFormat);
            
            font?.Dispose();
        }
    }
}