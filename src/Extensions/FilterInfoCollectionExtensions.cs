using System;
using AForge.Video.DirectShow;

namespace LiveSplit.PixelSplitter.Extensions
{
    internal static class FilterInfoCollectionExtensions
    {
        public static FilterInfo FirstOrDefault(this FilterInfoCollection collection, Func<FilterInfo, bool> predicate)
        {
            foreach (FilterInfo filter in collection)
            {
                if (predicate(filter)) return filter;
            }
            return null;
        }
        public static FilterInfo First(this FilterInfoCollection collection, Func<FilterInfo, bool> predicate)
        {
            foreach (FilterInfo filter in collection)
            {
                if (predicate(filter)) return filter;
            }
            throw new Exception("Target item not found in the collection.");
        }
    }
}