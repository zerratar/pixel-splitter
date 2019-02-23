using System.Collections.Generic;
using System.Linq;
using LiveSplit.PixelSplitter.Models;
using LiveSplit.PixelSplitter.Providers;

namespace LiveSplit.PixelSplitter.Comparer
{
    internal class ActionMatchComparer : IActionMatchComparer
    {
        private readonly IGameImageMatchComparer comparer;
        private readonly IPixelSplitterSettingsProvider settingsProvider;

        public ActionMatchComparer(IGameImageMatchComparer comparer, IPixelSplitterSettingsProvider settingsProvider)
        {
            this.comparer = comparer;
            this.settingsProvider = settingsProvider;
        }

        public bool AnyMatch(IMaskedGameImage gameImageSource, IReadOnlyList<GameImageMatchAction> actions)
        {
            var settings = settingsProvider.Get();
            foreach (var match in actions.SelectMany(x => x.ComparisonImages))
            {
                if (comparer.IsMatch(gameImageSource, match, settings.ImageMatchValuePercent))
                {
                    return true;
                }
            }
            return false;
        }
    }
}