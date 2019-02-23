using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveSplit.PixelSplitter.Providers;

namespace LiveSplit.PixelSplitter.Controls
{
    public partial class MaskEditorControl : UserControl
    {
        private readonly IGameImageSource gameImageSource;
        private readonly List<RectangleF> masks;
        private bool isMouseDown;

        private Point selectionStart;

        private Rectangle activeMask;
        private RectangleF selectedMask;

        public MaskEditorControl(IGameImageSource gameImageSource, List<RectangleF> masks)
        {
            this.gameImageSource = gameImageSource;
            this.masks = masks ?? new List<RectangleF>();
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        public List<RectangleF> Masks => masks;

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Delete && !this.selectedMask.IsEmpty)
            {
                this.masks.Remove(selectedMask);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.isMouseDown = true;
            this.selectedMask = new RectangleF();
            this.selectionStart = new Point(e.X, e.Y);
            this.activeMask = new Rectangle(e.X, e.Y, 0, 0);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (this.isMouseDown)
            {
                if (this.activeMask.Width > 5 && this.activeMask.Height > 5) // some people have shaky hands.
                {
                    var w = (float)this.activeMask.Width / this.Width;
                    var h = (float)this.activeMask.Height / this.Height;
                    var x = (float)this.activeMask.X / this.Width;
                    var y = (float)this.activeMask.Y / this.Height;
                    this.selectedMask = new RectangleF(x, y, w, h);
                    this.masks.Add(this.selectedMask);
                    this.activeMask = new Rectangle();
                }
                else
                {
                    var mask = this.masks.FirstOrDefault(x =>
                    {
                        var i = (float)e.X / this.Width;
                        var f = (float)e.Y / this.Height;
                        return x.Contains(i, f);
                    });
                    this.selectedMask = mask;
                }
            }

            this.isMouseDown = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this.isMouseDown)
            {
                var mX = Math.Max(0, Math.Min(e.X, this.Width));
                var mY = Math.Max(0, Math.Min(e.Y, this.Height));
                var w = Math.Abs(mX - this.selectionStart.X);
                var h = Math.Abs(mY - this.selectionStart.Y);
                var x = this.selectionStart.X;
                var y = this.selectionStart.Y;
                if (mX < this.selectionStart.X) x = mX;
                if (mY < this.selectionStart.Y) y = mY;
                this.activeMask = new Rectangle(x, y, w, h);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Call the OnPaint method of the base class.  
            base.OnPaint(e);

            this.DrawGameImage(e);

            this.DrawMasks(e);

            this.Invalidate();
        }

        private void DrawMasks(PaintEventArgs e)
        {
            using (System.Drawing.Pen green = new System.Drawing.Pen(Color.Green))
            using (System.Drawing.Pen red = new System.Drawing.Pen(Color.Red))
            {
                foreach (var mask in masks)
                {
                    var rect = new Rectangle(
                        (int)(mask.X * this.Width),
                        (int)(mask.Y * this.Height),
                        (int)(mask.Width * this.Width),
                        (int)(mask.Height * this.Height));

                    var pen = this.selectedMask == mask ? green : red;
                    e.Graphics.DrawRectangle(pen, rect);
                }

                if (!activeMask.IsEmpty)
                {
                    e.Graphics.DrawRectangle(green, activeMask);
                }
            }
        }

        private void DrawGameImage(PaintEventArgs e)
        {
            if (gameImageSource.Available)
            {
                try
                {
                    var bmp = gameImageSource.Frame;
                    e.Graphics.DrawImage(
                        bmp,
                        new Rectangle(0, 0, this.Width, this.Height),
                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                        GraphicsUnit.Pixel);
                }
                catch
                {
                }
            }
        }
    }
}

