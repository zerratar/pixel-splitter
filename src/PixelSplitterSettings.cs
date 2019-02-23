using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace LiveSplit.PixelSplitter
{
    public class PixelSplitterSettings
    {
        private List<RectangleF> imageMatchMask;
        private float imageMatchValuePercent;
        private bool modified;
        private byte[,] imageBitMask;
        private string captureDevice;
        private Bitmap maskBitmap;

        public PixelSplitterSettings(
            string captureDevice,
            float imageMatchValuePercent,
            List<RectangleF> imageMatchMask)
        {
            this.captureDevice = captureDevice;
            this.imageMatchValuePercent = imageMatchValuePercent;
            this.imageMatchMask = imageMatchMask;
        }

        public string CaptureDevice
        {
            get => this.captureDevice;
            set
            {
                this.captureDevice = value;
                this.modified = true;
            }
        }

        public float ImageMatchValuePercent
        {
            get => imageMatchValuePercent;
            set
            {
                imageMatchValuePercent = value;
                this.modified = true;
            }
        }

        public List<RectangleF> ImageMatchMask
        {
            get => imageMatchMask;
            set
            {
                imageMatchMask = value;
                this.modified = true;
            }
        }

        public bool Modified => this.modified;

        public Bitmap GetMaskBitmap(int width, int height, PixelFormat pixelFormat)
        {
            if (this.modified || this.maskBitmap == null || maskBitmap.Width * maskBitmap.Height != width * height || maskBitmap.PixelFormat != pixelFormat)
            {
                this.maskBitmap = new Bitmap(width, height, pixelFormat);
                using (var gfx = Graphics.FromImage(maskBitmap))
                {
                    gfx.FillRectangle(Brushes.Black, 0, 0, maskBitmap.Width, maskBitmap.Height);
                }

                using (var locker = new BitmapLocker(this.maskBitmap))
                {
                    foreach (var mask in this.imageMatchMask)
                    {
                        var w = mask.Width * width;
                        var h = mask.Height * height;
                        var ox = (int)(mask.X * width);
                        var oy = (int)(mask.Y * height);

                        for (var x = 0; x < w; ++x)
                            for (var y = 0; y < h; ++y)
                            {
                                locker.SetPixel(ox + x, oy + y, Color.Transparent);
                            }
                    }
                }

                this.Update();
            }
            return this.maskBitmap;
        }

        public byte[,] GetImageBitMask(int width, int height)
        {
            if (this.modified || this.imageBitMask == null || this.imageBitMask.LongLength != width * height)
            {
                this.imageBitMask = new byte[height, width];
                foreach (var mask in this.imageMatchMask)
                {
                    var w = mask.Width * width;
                    var h = mask.Height * height;
                    var ox = (int)(mask.X * width);
                    var oy = (int)(mask.Y * height);
                    for (var x = 0; x < w; ++x)
                        for (var y = 0; y < h; ++y)
                        {
                            this.imageBitMask[oy + y, ox + x] = 1;
                        }
                }
                this.Update();
            }

            return this.imageBitMask;
        }

        public void Update()
        {
            this.modified = false;
        }
    }
}