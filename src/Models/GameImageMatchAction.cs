using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LiveSplit.PixelSplitter.Models
{
    public class GameImageMatchAction
    {
        public GameImageMatchAction()
        {
            this.Id = Guid.NewGuid();
            this.ComparisonImages = new List<SplitComparisonImage>();
        }

        [JsonConstructor]
        public GameImageMatchAction(Guid id, GameImageMatchActionType type, string splitName, IList<SplitComparisonImage> comparisonImages)
        {
            Id = id;
            Type = type;
            SplitName = splitName;
            ComparisonImages = comparisonImages;
        }

        public Guid Id { get; }
        public GameImageMatchActionType Type { get; set; }
        public string SplitName { get; set; }
        public IList<SplitComparisonImage> ComparisonImages { get; set; }

        public GameImageMatchAction Clone()
        {
            return JsonConvert.DeserializeObject<GameImageMatchAction>(JsonConvert.SerializeObject(this));
        }
    }
}