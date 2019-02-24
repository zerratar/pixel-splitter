using System;
using System.Collections.Generic;
using LiveSplit.PixelSplitter.Models;

namespace LiveSplit.PixelSplitter.Repositories
{
    public interface IActionRepository
    {
        string Identifier { get; }
        IReadOnlyList<GameImageMatchAction> All();
        GameImageMatchAction Find(Func<GameImageMatchAction, bool> find);
        bool TryGet(GameImageMatchActionType type, out IReadOnlyList<GameImageMatchAction> actions);
        void Store(GameImageMatchAction action);
        void Remove(GameImageMatchAction action);
        void Move(int indexFrom, int indexTo);
        void Save();
    }
}