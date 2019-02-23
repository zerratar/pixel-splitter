using AForge.Video.DirectShow;

namespace LiveSplit.PixelSplitter.Models
{
    internal class CaptureDevice
    {
        private FilterInfo info;

        public CaptureDevice(FilterInfo info)
        {
            this.info = info;
        }

        public string Name => info.Name;

        public string MonikerString => info.MonikerString;

        public override string ToString()
        {
            return this.Name;
        }
    }
}