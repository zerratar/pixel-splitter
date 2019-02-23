using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.PixelSplitter.Controls
{
    public partial class MaskEditorWindow : Form
    {
        private MaskEditorControl maskEditorControl;

        public MaskEditorWindow(IGameImageSource gameImageSource, List<RectangleF> masks)
        {
            InitializeComponent();
            this.maskEditorControl = new MaskEditorControl(gameImageSource, masks);
            this.maskEditorControl.Dock = DockStyle.Fill;
            this.Controls.Add(maskEditorControl);
        }

        public List<RectangleF> Masks => this.maskEditorControl.Masks;

        private void MaskEditorWindow_Load(object sender, EventArgs e)
        {
        }
    }
}
