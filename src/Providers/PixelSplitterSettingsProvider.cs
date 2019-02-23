using LiveSplit.PixelSplitter.Controls;

namespace LiveSplit.PixelSplitter.Providers
{
    internal class PixelSplitterSettingsProvider : IPixelSplitterSettingsProvider
    {
        private readonly ComponentSettings componentSettings;
        
        public PixelSplitterSettingsProvider(ComponentSettings componentSettings)
        {
            this.componentSettings = componentSettings;
        }
        public PixelSplitterSettings Get()
        {
            return componentSettings.GetSplitterSettings();
        }
    }
}