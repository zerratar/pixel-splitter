using System;
using LiveSplit.Model;
using LiveSplit.PixelSplitter;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(Factory))]

namespace LiveSplit.PixelSplitter
{
    public class Factory : IComponentFactory
    {
        public string ComponentName => "Pixel based Auto Splitter";        
        public string Description => "Allows the use of visual cues to do auto splitting";
        public ComponentCategory Category => ComponentCategory.Control;
        public Version Version => Version.Parse("1.0.0");

        public string UpdateName => ComponentName;
        public string UpdateURL => "";
        public string XMLURL => "";

        public IComponent Create(LiveSplitState state) => new PixelSplitterComponent(state);
    }
}
