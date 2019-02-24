using System;
using LiveSplit.Model;
using LiveSplit.Model.Input;

namespace LiveSplit.PixelSplitter
{
    internal class LiveSplitController
    {
        private readonly LiveSplitState state;
        private TimerModel timer;
        private bool initialized;

        public LiveSplitController(LiveSplitState state)
        {
            this.state = state;
        }

        public void Initialize()
        {
            this.timer = new TimerModel { CurrentState = this.state };
            this.state.OnSplit += OnSplit;
            this.state.OnSkipSplit += OnSkip;
            this.initialized = true;
            LastSplitName = this.CurrentSplitName;
        }

        private void OnSkip(object sender, EventArgs eventArgs)
        {
            LastSplitName = this.CurrentSplitName;
        }

        private void OnSplit(object sender, EventArgs eventArgs)
        {
            LastSplitName = this.CurrentSplitName;
        }

        public bool Initialized => initialized;

        public string GameName => this.state.Run?.GameName;

        public string CategoryName => this.state.Run?.CategoryName;

        public string CurrentSplitName
        {
            get
            {
                if (this.state.CurrentSplit == null)
                {
                    return this.state.Run[0].Name;
                }
                return this.state.CurrentSplit.Name;
            }
        }

        public string LastSplitName { get; private set; }

        public string Identifier => $"{GameName}-{CategoryName}";



        public void RegisterHotkeys(CompositeHook hook)
        {
            this.state.Settings.RegisterHotkeys(hook);
        }

        public void Pause()
        {
            this.timer.Pause();
        }

        public void Split()
        {
            this.timer.Split();
        }

        public void Skip()
        {
            this.timer.SkipSplit();
        }

        public void Start()
        {
            this.timer.Start();
        }

        public void End()
        {
            this.timer.Reset(true);
        }

        public void Abort()
        {
            this.timer.Reset(false);
        }

        public void Resume()
        {
            this.timer.Pause();
        }

        public void UndoSplit()
        {
            this.timer.UndoSplit();
        }
    }
}