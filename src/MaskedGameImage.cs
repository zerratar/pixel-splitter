using System.Drawing;

namespace LiveSplit.PixelSplitter
{
    internal class MaskedGameImage : IMaskedGameImage
    {
        public MaskedGameImage(Bitmap image)
        {
            Image = image;
        }

        public Bitmap Image { get; }
        public int Width => Image.Width;
        public int Height => Image.Height;


        public void Save(string path)
        {
            this.Image.Save(path);
        }
    }
}