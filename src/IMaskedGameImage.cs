using System.Drawing;

namespace LiveSplit.PixelSplitter
{
    public interface IMaskedGameImage
    {
        int Width { get; }
        int Height { get; }
        Bitmap Image { get; }
        void Save(string path);
    }
}