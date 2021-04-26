using System.Drawing;
using System.Windows.Forms;

namespace UraniumVisualizer
{
    public partial class VisualizerForm : Form
    {
        public VisualizerForm()
        {
            var panel = new Panel();
            panel.Paint += Draw;
            Controls.Add(panel);
            InitializeComponent();
        }

        private void Draw(object sender, PaintEventArgs args)
        {
            args.Graphics.FillRectangle(Brushes.Red, new Rectangle(50, 50, 100, 100));
        }

        private void VisualizerForm_Load(object sender, System.EventArgs e)
        {

        }
    }
}