using System;
using System.Drawing;
using System.Windows.Forms;

namespace UraniumVisualizer
{
    public partial class VisualizerForm : Form
    {
        private readonly IVisualizer visualizer;
        private bool isMoving;

        private int previousMouseX;
        private int previousMouseY;

        public VisualizerForm()
        {
            InitializeComponent();
            visualizer = new Visualizer {FunctionHeight = 30};
            visualizer.OpenSessionFile("../../ParserTests/test1.ups");
            Invalidate();
        }

        private static Rectangle ConvertRectangle(FunctionRectangle rectangle)
        {
            return new Rectangle((int) rectangle.Left, (int) rectangle.Top,
                (int) rectangle.Width, (int) rectangle.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            foreach (var rectangle in visualizer.GetRectangles())
            {
                var brush = new SolidBrush(ColorPalette.GetColor(rectangle.Record.Name));
                graphics.FillRectangle(brush, ConvertRectangle(rectangle));
                var left = (int) Math.Max(rectangle.Left, ClientRectangle.Left);
                var right = (int) Math.Min(rectangle.Right, ClientRectangle.Right);
                var top = (int) rectangle.Top;
                var bottom = (int) rectangle.Bottom;
                DrawText(rectangle.Record.Name, graphics, new Rectangle(left, top,
                    Math.Abs(right - left), Math.Abs(bottom - top)),
                    Brushes.Black);
            }

            if (visualizer.SelectedFunction.HasValue)
            {
                graphics.DrawRectangle(Pens.White, ConvertRectangle(visualizer.SelectedFunction.Value));
            }
        }

        private static void DrawText(string text, Graphics graphics, Rectangle rectangle, Brush brush)
        {
            using (var font = new Font("Arial", 16))
            {
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
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
                    visualizer.SelectFunctionUnderCursor(e.X, e.Y);
                    break;
                case MouseButtons.Right:
                    isMoving = true;
                    previousMouseX = e.X;
                    previousMouseY = e.Y;
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
            visualizer.ApplyMoving(e.X - previousMouseX, e.Y - previousMouseY);
            previousMouseX = e.X;
            previousMouseY = e.Y;

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            visualizer.ApplyZooming(Math.Sign(e.Delta) * 1.15);

            Invalidate();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            visualizer.ResizeScreen(Width, Height);
            DoubleBuffered = true;
            BackColor = Color.FromArgb(255, Color.FromArgb(0x252525));

            Invalidate();
        }
    }
}