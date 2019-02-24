using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LiveSplit.PixelSplitter.Models;

namespace LiveSplit.PixelSplitter.Repositories
{
    internal class JsonBasedActionRepository : IActionRepository
    {
        private readonly List<GameImageMatchAction> loadedActions = new List<GameImageMatchAction>();
        private readonly object repoLock = new object();
        private readonly string path;

        public JsonBasedActionRepository(string identifier, string path)
        {
            this.path = path;
            this.Identifier = identifier;
            this.LoadRepository();
        }

        private void LoadRepository()
        {
            if (!System.IO.Directory.Exists(this.path))
            {
                System.IO.Directory.CreateDirectory(this.path);
                return;
            }

            if (!System.IO.File.Exists(RepoFile))
            {
                return;
            }

            lock (repoLock)
            {
                var data = System.IO.File.ReadAllText(RepoFile);
                var actions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GameImageMatchAction>>(data);
                this.loadedActions.AddRange(actions);
            }
        }

        public string Identifier { get; }

        public IReadOnlyList<GameImageMatchAction> All()
        {
            lock (repoLock)
            {
                return loadedActions;
            }
        }

        public GameImageMatchAction Find(Func<GameImageMatchAction, bool> find)
        {
            lock (repoLock)
            {
                return this.loadedActions.FirstOrDefault(find);
            }
        }

        public bool TryGet(GameImageMatchActionType type, out IReadOnlyList<GameImageMatchAction> actions)
        {
            lock (repoLock)
            {
                actions = this.loadedActions.Where(x => (type & x.Type) == x.Type).ToList();
                return actions.Count > 0;
            }
        }

        public void Store(GameImageMatchAction action)
        {
            lock (repoLock)
            {
                var existing = loadedActions.FirstOrDefault(x => x.Id == action.Id);
                if (existing == null)
                {
                    loadedActions.Add(action);
                }
                else
                {
                    existing.ComparisonImages = action.ComparisonImages;
                    existing.SplitName = action.SplitName;
                    existing.Type = action.Type;
                }
                Save();
            }
        }

        public void Remove(GameImageMatchAction action)
        {
            lock (repoLock)
            {
                loadedActions.Remove(action);
                Save();
            }
        }

        public void Move(int indexFrom, int indexTo)
        {
            lock (repoLock)
            {
                var item = this.loadedActions[indexFrom];
                this.loadedActions.Remove(item);
                this.loadedActions.Insert(indexTo, item);
                this.Save();
            }
        }

        public void Save()
        {
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(this.loadedActions);
            System.IO.File.WriteAllText(RepoFile, data);

            foreach (var action in this.loadedActions)
            {
                foreach (var image in action.ComparisonImages)
                {
                    if (!string.IsNullOrEmpty(image.SourceImagePath) && !System.IO.File.Exists(image.SourceImagePath))
                    {
                        image.SourceImage?.Save(image.SourceImagePath);
                    }
                }
            }
        }

        private string RepoFile => Path.Combine(this.path, "data.json");
    }
}