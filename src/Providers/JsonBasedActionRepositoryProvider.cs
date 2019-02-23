using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LiveSplit.PixelSplitter.Repositories;

namespace LiveSplit.PixelSplitter.Providers
{
    public class JsonBasedActionRepositoryProvider : IActionRepositoryProvider
    {
        public const string RepositoriesFolder = "PixelSplitter";

        private readonly ConcurrentDictionary<string, JsonBasedActionRepository> loadedRepositories
            = new ConcurrentDictionary<string, JsonBasedActionRepository>();

        public IActionRepository Get(string gameName, string categoryName)
        {
            var key = GetKey(gameName, categoryName);
            if (loadedRepositories.TryGetValue(key, out var repo))
            {
                return repo;
            }

            if (!System.IO.Directory.Exists(RepositoriesFolder))
                System.IO.Directory.CreateDirectory(RepositoriesFolder);

            return loadedRepositories[key] = new JsonBasedActionRepository(key, Path.Combine(RepositoriesFolder, gameName, categoryName));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetKey(string gameName, string categoryName)
        {
            return $"{gameName}-${categoryName}";
        }
    }
}