using System.Drawing;
using Newtonsoft.Json;

namespace LiveSplit.PixelSplitter.Models
{
    public class SplitComparisonImage
    {
        private Bitmap sourceImage;

        public SplitComparisonImage(string sourceImagePath)
        {
            SourceImagePath = sourceImagePath;
        }

        public string SourceImagePath { get; set; }

        [JsonIgnore]
        public Bitmap SourceImage
        {
            get
            {
                if (sourceImage == null && System.IO.File.Exists(SourceImagePath))
                {
                    sourceImage = (Bitmap)Bitmap.FromFile(SourceImagePath);
                }

                return sourceImage;
            }
        }
    }
}