using LiveSplit.PixelSplitter.Models;

namespace LiveSplit.PixelSplitter.Comparer
{
    internal interface IGameImageMatchComparer
    {
        float GetMatchPercent(IMaskedGameImage gameImage, SplitComparisonImage splitImage);
        bool IsMatch(IMaskedGameImage gameImage, SplitComparisonImage splitImage, float minPercent);
    }
}