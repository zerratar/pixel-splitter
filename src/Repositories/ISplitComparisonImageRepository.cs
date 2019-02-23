using System.Collections.Generic;
using System.Drawing;
using LiveSplit.PixelSplitter.Models;

namespace LiveSplit.PixelSplitter.Repositories
{
    internal interface ISplitComparisonImageRepository
    {
        IReadOnlyList<SplitComparisonImage> Get(string splitName);
        void Store(string splitName, Image image);
        void Remove(string splitName, int index);
    }
}