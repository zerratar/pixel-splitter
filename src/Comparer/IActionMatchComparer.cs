using System.Collections.Generic;
using LiveSplit.PixelSplitter.Models;

namespace LiveSplit.PixelSplitter.Comparer
{
    internal interface IActionMatchComparer
    {
        bool AnyMatch(IMaskedGameImage image, IReadOnlyList<GameImageMatchAction> actions);
    }
}