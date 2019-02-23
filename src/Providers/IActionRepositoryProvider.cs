using LiveSplit.PixelSplitter.Repositories;

namespace LiveSplit.PixelSplitter.Providers
{
    public interface IActionRepositoryProvider
    {
        IActionRepository Get(string gameName, string categoryName);
    }
}