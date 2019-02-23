using System;
using System.Drawing;
using LiveSplit.PixelSplitter.Models;

namespace LiveSplit.PixelSplitter
{
    public interface IGameImageSource : IDisposable
    {
        bool Available { get; }
        bool HasNewFrame { get; }
        Bitmap Frame { get; }
        void Next();
        void SaveFrame(string path);
        IMaskedGameImage GetMaskedFrame(PixelSplitterSettings settings);
    }
}