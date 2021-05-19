using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace UraniumVisualizer
{
    public partial class VisualizerForm : Form
    {

        public VisualizerForm()
        {
            InitializeComponent();

            panel1.Paint += DrawFunctions;
            panel1.Paint += DrawFrame;
            panel2.Paint += DrawTimeLine;
            Controls.Add(panel1);
            
            var parser = new FileParser(@"C:\Users\1\source\repos\AT-33\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\test1.ups");
            funcs = parser.Parse().ToList();

            /// Test functions
            funcs.Add(new FunctionRecord("first", 1, 100, 1500));
            funcs.Add(new FunctionRecord("second", 2, 5000, 400));
            funcs.Add(new FunctionRecord("third", 3, 1000, 400));
            for (var i = 0; i < 100; i++)
            {
                funcs.Add(new FunctionRecord("first" + i.ToString(), 10 + i, i * 500, 300));
            }
            ///
        }

        public List<FunctionRecord> funcs = new List<FunctionRecord>();
        public static readonly int[] colors = {
            0x000000,
            0x000033,
            0x000066,
            0x000099,
            0x0000CC,
            0x0000FF,
            0x003300,
            0x003333,
            0x003366,
            0x003399,
            0x0033CC,
            0x0033FF,
            0x006600,
            0x006633,
            0x006666,
            0x006699,
            0x0066CC,
            0x0066FF,
            0x009900,
            0x009933,
            0x009966,
            0x009999,
            0x0099CC,
            0x0099FF,
            0x00CC00,
            0x00CC33,
            0x00CC66,
            0x00CC99,
            0x00CCCC,
            0x00CCFF,
            0x00FF00,
            0x00FF33,
            0x00FF66,
            0x00FF99,
            0x00FFCC,
            0x00FFFF,
            0x330000,
            0x330033,
            0x330066,
            0x330099,
            0x3300CC,
            0x3300FF,
            0x333300,
            0x333333,
            0x333366,
            0x333399,
            0x3333CC,
            0x3333FF,
            0x336600,
            0x336633,
            0x336666,
            0x336699,
            0x3366CC,
            0x3366FF,
            0x339900,
            0x339933,
            0x339966,
            0x339999,
            0x3399CC,
            0x3399FF,
            0x33CC00,
            0x33CC33,
            0x33CC66,
            0x33CC99,
            0x33CCCC,
            0x33CCFF,
            0x33FF00,
            0x33FF33,
            0x33FF66,
            0x33FF99,
            0x33FFCC,
            0x33FFFF,
            0x660000,
            0x660033,
            0x660066,
            0x660099,
            0x6600CC,
            0x6600FF,
            0x663300,
            0x663333,
            0x663366,
            0x663399,
            0x6633CC,
            0x6633FF,
            0x666600,
            0x666633,
            0x666666,
            0x666699,
            0x6666CC,
            0x6666FF,
            0x669900,
            0x669933,
            0x669966,
            0x669999,
            0x6699CC,
            0x6699FF,
            0x66CC00,
            0x66CC33,
            0x66CC66
        };

        private int oldX, oldY;
        private double scale = 1;
        private bool isMoving;

        private int cursorPosX, cursorPosY;
        private FunctionRecord funcUnderCursor = new FunctionRecord("null",0,0,0);

        private void Panel1_MouseDown(object sender, MouseEventArgs args)
        {
            isMoving = true;
            oldX = args.X;
            oldY = args.Y;
            cursorPosX = args.X;
            cursorPosY = args.Y;
            var t = SearchFuncUnderCursor(funcs, cursorPosX, cursorPosY, scale);
            if (t != null)
            {
                funcUnderCursor = t;
                panel1.Invalidate();
            }
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs args)
        {
            if (isMoving)
            {
                if (panel1.Left + args.X - oldX <= 0)
                {
                    panel1.Left += args.X - oldX;
                    panel1.Width += args.X;
                    panel2.Left += args.X - oldX;
                    panel2.Width += args.X;
                }

                if (panel1.Top + args.Y - oldY <= 0)
                {
                    panel1.Top += args.Y - oldY;
                    panel1.Height += args.Y;
                }
            }
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs args)
        {
            isMoving = false;
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs args)
        {
            if (args.Delta > 0)
            {
                scale *= 1.15;
                panel1.Invalidate();
                panel2.Invalidate();
            }
            if (args.Delta < 0)
            {
                scale /= 1.15;
                panel1.Invalidate();
                panel2.Invalidate();
            }
        }

        private void DrawTimeLine(object sender, PaintEventArgs args)
        {
            args.Graphics.DrawLine(new Pen(Color.DarkSlateGray, 7), 0, 22, panel1.Width, 22);
            int i, j, k;
            if (scale <= 0.05) // each 30 seconds
            {
                for (k = 30; k < panel1.Width; k += 30)
                {
                    args.Graphics.DrawLine(new Pen(Color.White, 2), (float)(k * 1000 * scale), 13, (float)(k * 1000 * scale), 25);
                    args.Graphics.DrawString(k.ToString() + " sec", new Font("Arial", 15, FontStyle.Regular), new SolidBrush(Color.Orange),
                        (float)(k * 1000 * scale), 2);
                }
            }

            if (scale <= 0.15 && scale >= 0.05) // each 5 seconds
            {
                for (k = 5; k < panel1.Width; k += 5)
                {
                    args.Graphics.DrawLine(new Pen(Color.White, 2), (float)(k * 1000 * scale), 13, (float)(k * 1000 * scale), 25);
                    args.Graphics.DrawString(k.ToString() + " sec", new Font("Arial", 15, FontStyle.Regular), new SolidBrush(Color.Orange),
                        (float)(k * 1000 * scale), 2);
                }
            }

            if (scale <= 5 && scale >= 0.15) // each 100 milliseconds
            {
                for (i = 500; i < panel1.Width; i += 500)
                {
                    args.Graphics.DrawLine(new Pen(Color.White, 2), (float)(i * scale), 13, (float)(i * scale), 25);
                    args.Graphics.DrawString(i.ToString() + " ms", new Font("Arial", 15, FontStyle.Regular), new SolidBrush(Color.White),
                        (float)(i * scale), 2);
                }
            }

            if (scale <= 8 && scale >= 2) // each 50 milliseconds 
            {
                for (i = 50; i < panel1.Width; i += 50)
                {
                    args.Graphics.DrawLine(new Pen(Color.White, 2), (float)(i * scale), 13, (float)(i * scale), 25);
                    args.Graphics.DrawString(i.ToString() + " ms", new Font("Arial", 15, FontStyle.Regular),
                        new SolidBrush(Color.White), (float)(i * scale), 2);
                }
            }

            if (scale > 8) // each 10 milliseconds
            {
                for (j = 10; j < panel1.Width; j += 10)
                {
                    args.Graphics.DrawLine(new Pen(Color.White, 2), (float)(j * scale), 13, (float)(j * scale), 25);
                    args.Graphics.DrawString((j).ToString() + " ms", new Font("Arial", 15, FontStyle.Regular),
                        new SolidBrush(Color.White), (float)(j * scale), 2);
                }
            }
        }

        private void DrawFrame(object sender, PaintEventArgs args)
        {
            var func = funcUnderCursor;
            args.Graphics.DrawRectangle(new Pen(Color.Black, 3), new Rectangle((int)((func.PositionX) * scale),
                30 + (int)(func.PositionY * 30), (int)(func.Duration * scale), 30));
        }

        private void DrawFunctions(object sender, PaintEventArgs args)
        {
            for (var i = 0; i < funcs.Count; i++)
            {
                args.Graphics.FillRectangle(new SolidBrush(GetColor(funcs[i].Name)), new Rectangle((int)(funcs[i].PositionX * scale),
                    30 + (int)(funcs[i].PositionY * 30), (int)(funcs[i].Duration * scale), 30));
                args.Graphics.DrawString(funcs[i].Name, new Font("Arial", 10,
                    FontStyle.Regular,GraphicsUnit.Pixel), new SolidBrush(Color.Black), (float)((funcs[i].PositionX+30)*scale), funcs[i].PositionY*30+30);
            }
        }

        private static Color GetColor(string functionName)
        {
            var hash = (uint)functionName.GetHashCode();
            return Color.FromArgb(255, Color.FromArgb(colors[hash % colors.Length]));
        }

        private static FunctionRecord SearchFuncUnderCursor(List<FunctionRecord> funcs, int cursorX, int cursorY, double scale)
        {
            for (var i = 0; i < funcs.Count; i++)
            {
                if (cursorX >= funcs[i].PositionX * scale && cursorX <= (funcs[i].PositionX * scale + funcs[i].Duration * scale) 
                    && cursorY >= 30 + funcs[i].PositionY * 30 && cursorY <= 30 + funcs[i].PositionY * 30 + 30)
                    return funcs[i];
            }
            return null;
        }

        private void VisualizerForm_Resize(object sender, EventArgs e)
        {
            Graphics g = CreateGraphics();
            panel1.Width = (int)g.VisibleClipBounds.Width;
            panel1.Height = (int)g.VisibleClipBounds.Height;
            panel1.Invalidate();
        }

        private void VisualizerForm_Load(object sender, System.EventArgs e)
        {

        }
    }
}