using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;
using LiveSplit.PixelSplitter.Models;

namespace LiveSplit.PixelSplitter.Comparer
{
    internal class GameImageMatchComparer : IGameImageMatchComparer
    {
        public float GetMatchPercent(IMaskedGameImage gameImage, SplitComparisonImage splitImage)
        {
            try
            {
                if (gameImage.Image == null || splitImage.SourceImage == null) return 0;
                //var difference = new Difference(splitImage.SourceImage);
                //var imageDiff = difference.Apply(gameImage.Frame);
                //var final = imageDiff;
                var image1 = gameImage.Image;
                var image2 = splitImage.SourceImage;

                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0);
                // compare two images
                TemplateMatch[] matchings = tm.ProcessImage(image1, image2);

                if (matchings.Length == 0) return 0;

                //// check similarity level
                //if (matchings[0].Similarity > 0.97f)
                //{
                //    image1.Save("c:\\debug_image2.png", ImageFormat.Png);
                //    // do something with quite similar images
                //}

                return matchings[0].Similarity;
            }
            catch
            {
                return 0;
            }
        }

        public bool IsMatch(IMaskedGameImage gameImage, SplitComparisonImage splitImage, float minPercent)
        {
            return GetMatchPercent(gameImage, splitImage) >= minPercent;
        }
    }
}