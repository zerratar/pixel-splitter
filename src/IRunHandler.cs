using System;

namespace LiveSplit.PixelSplitter
{
    internal interface IRunHandler : IDisposable
    {
        bool IsReady { get; }
        void Init(string captureSource);
        void NotReady();
        void NotRunning();
        void Ended();
        void Paused();
        void Running();
    }
}