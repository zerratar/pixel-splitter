namespace LiveSplit.PixelSplitter.Providers
{
    public interface IGameImageProvider
    {
        IGameImageSource Get(string key);
    }
}