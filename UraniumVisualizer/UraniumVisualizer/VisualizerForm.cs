using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UraniumVisualizer
{
    public partial class VisualizerForm : Control
    {
        public event Action<List<FunctionRecord>, Dictionary<string,(int count, double maxTime)>> SelectionChanged;
        private readonly IVisualizer visualizer;
        private static readonly List<double> powersOfTen = new List<double>();
        private static readonly List<(int factor, string postfix)> conversions =
            new List<(int factor, string postfix)>();
        
        private const int MarksOnScreen = 25;
        private const int TimelineHeight = 20;
        
        private bool isMoving;

        private Size previousMousePoint;

        private readonly Dictionary<string, Brush> nameBrushes = new Dictionary<string, Brush>();

        public VisualizerForm()
        {
            InitializeComponent();
            visualizer = new Visualizer {FunctionHeight = 30};
            Invalidate();
        }

        static VisualizerForm()
        {
            for (var i = 0.000_000_001; i <= 1000; i *= 10)
            {
                powersOfTen.Add(i);
                if (i < 0.000_001)
                    conversions.Add((1_000_000_000, "ns"));
                else if (i < 0.001)
                    conversions.Add((1_000_000, "μs"));
                else if (i < 1)
                    conversions.Add((1_000, "ms"));
                else
                    conversions.Add((1, "s"));
            }
        }

        public void OpenSessionFile(string filePath)
        {
            visualizer.DeselectAll();
            visualizer.OpenSessionFile(filePath);
            Invalidate();
        }

        private static Rectangle ConvertRectangle(FunctionRectangle rectangle)
        {
            return new Rectangle((int) rectangle.Left, (int) rectangle.Top + TimelineHeight,
                (int) rectangle.Width, (int) rectangle.Height);
        }

        private Brush GetFunctionBrush(string fnName)
        {
            if (nameBrushes.TryGetValue(fnName, out var brush))
                return brush;
            brush = new SolidBrush(ColorPalette.GetColor(fnName));
            nameBrushes[fnName] = brush;
            return nameBrushes[fnName];
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            foreach (var rectangle in visualizer.GetRectangles())
            {
                var brush = GetFunctionBrush(rectangle.Record.Name);
                var rect = ConvertRectangle(rectangle);
                rect.Intersect(ClientRectangle);
                
                if (!ClientRectangle.IntersectsWith(rect))
                    continue;
                
                graphics.FillRectangle(brush, rect);
                graphics.DrawRectangle(Pens.White, rect);
                var left = Math.Max(rect.Left, ClientRectangle.Left);
                var right = Math.Min(rect.Right, ClientRectangle.Right);
                var top = rect.Top;
                var bottom = rect.Bottom;
                DrawText(rectangle.Record.Name, graphics, new Rectangle(left, top,
                    right - left, bottom - top), Brushes.Black, 12);
            }
            
            if (visualizer.SelectedFunction.HasValue)
            {
                var rect = ConvertRectangle(visualizer.SelectedFunction.Value);
                rect.Intersect(ClientRectangle);
                var py = rect.Y;
                rect.Y = TimelineHeight;
                rect.Height = py - TimelineHeight + (int)visualizer.FunctionHeight;
                graphics.DrawRectangle(new Pen(Brushes.Black, 5), rect);
                graphics.DrawRectangle(new Pen(Brushes.White, 2), rect);
            }
            
            DrawTimeline(graphics);
        }

        private void DrawTimeline(Graphics graphics)
        {
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, Color.FromArgb(0x535353))),
                new Rectangle(new Point(0, 0), new Size(Width, TimelineHeight)));
            var secondsVisible = visualizer.RightSeconds - visualizer.LeftSeconds;
            var costIndex = powersOfTen.FindIndex(x => x >= secondsVisible / MarksOnScreen);
            if (costIndex == -1) costIndex = powersOfTen.Count - 1;
            DrawTimeMarks(graphics, costIndex);
        }

        private void DrawTimeMarks(Graphics graphics, int costIndex)
        {
            var cost = powersOfTen[costIndex];
            var start = visualizer.LeftSeconds - visualizer.LeftSeconds % cost;
            for (var i = start; i < visualizer.RightSeconds; i += cost)
            {
                var xl = (int) visualizer.SecondsToPixels(i);
                var xr = (int) visualizer.SecondsToPixels(i + cost);
                graphics.DrawLine(Pens.Black, xl, 0, xl, TimelineHeight);
                var rect = new Rectangle(xl, 0, xr - xl, TimelineHeight);
                DrawText(ConvertTime(i, costIndex), graphics, rect, Brushes.Black, 9, false);
            }
        }

        private static string ConvertTime(double seconds, int costIndex)
        {
            var (factor, postfix) = conversions[costIndex];
            return $"{seconds * factor}{postfix}";
        }
        
        private static void DrawText(string text, Graphics graphics, Rectangle rectangle, Brush brush, int emSize,
            bool center = true)
        {
            if (rectangle.Width < 0 || rectangle.Height < 0)
                return;
            
            using (var font = new Font("Arial", emSize))
            {
                var stringFormat = new StringFormat
                {
                    Alignment = center ? StringAlignment.Center : StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                graphics.DrawString(text, font, brush, rectangle, stringFormat);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            visualizer?.ResizeScreen(Width, Height);

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    visualizer.SelectFunctionUnderCursor(e.X, e.Y - TimelineHeight);
                    SelectionChanged?.Invoke(visualizer.SelectedFunction is null
                        ? null
                        : visualizer.GetAllSubFunctions(visualizer.SelectedFunction?.Record), visualizer.FunctionData);
                    break;
                case MouseButtons.Right:
                    isMoving = true;
                    previousMousePoint = new Size(e.Location);
                    break;
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right)
                isMoving = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!isMoving) return;
            var delta = e.Location - previousMousePoint;
            visualizer.ApplyMoving(delta.X, delta.Y);
            previousMousePoint = new Size(e.Location);

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            visualizer.ApplyZooming(e.Delta > 0 ? 1 / 1.15 : 1.15);

            Invalidate();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            visualizer.ResizeScreen(Width, Height);
            DoubleBuffered = true;
            BackColor = Color.FromArgb(255, Color.FromArgb(0x252525));

            Invalidate();
        }
    }
}