using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using LiveSplit.PixelSplitter.Comparer;
using LiveSplit.PixelSplitter.Controls;
using LiveSplit.PixelSplitter.Providers;
using LiveSplit.UI;
using LiveSplit.UI.Components;

namespace LiveSplit.PixelSplitter
{
    public class PixelSplitterComponent : LogicComponent
    {
        private readonly LiveSplitState state;
        private readonly ComponentSettings settings;
        private readonly LiveSplitController controller;

        private readonly IRunHandler runHandler;
        private readonly IActionMatchComparer actionMatcher;
        private Task runningTask;

        public PixelSplitterComponent(LiveSplitState state)
        {
            this.state = state;

            var repoProvider = new JsonBasedActionRepositoryProvider();
            var imageProvider = new CaptureDeviceGameImageProvider();


            settings = new ComponentSettings(state, repoProvider, imageProvider);
            var settingsProvider = new PixelSplitterSettingsProvider(settings);

            CaptureDeviceGameImageProvider.SettingsProvider = settingsProvider;

            controller = new LiveSplitController(this.state);

            actionMatcher = new ActionMatchComparer(new GameImageMatchComparer(), settingsProvider);
            runHandler = new PixelSplitterRunHandler(controller, repoProvider, actionMatcher, imageProvider, settingsProvider);
        }

        public override string ComponentName => "Pixel based Auto Splitter";

        public override Control GetSettingsControl(LayoutMode mode)
        {
            return settings;
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return settings.GetSettings(document);
        }

        public override void SetSettings(XmlNode settings)
        {
            this.settings.SetSettings(settings);
        }

        public override void Update(
            IInvalidator invalidator,
            LiveSplitState state,
            float width,
            float height,
            LayoutMode mode)
        {
            if (!controller.Initialized)
            {
                controller.Initialize();
                runHandler.Init(this.settings.GetSplitterSettings().CaptureDevice); //("Game Capture HD60 Pro (Video) (#01)");
            }

            Update();
        }

        private void Update()
        {
            if (runningTask != null && !runningTask.IsCompleted)
            {
                return;
            }

            this.runningTask = Task.Run(() =>
            {
                if (runHandler.IsReady)
                {
                    switch (this.state.CurrentPhase)
                    {
                        case TimerPhase.NotRunning:
                            runHandler.NotRunning();
                            return;
                        case TimerPhase.Ended:
                            runHandler.Ended();
                            return;
                        case TimerPhase.Paused:
                            runHandler.Paused();
                            return;
                        case TimerPhase.Running:
                            runHandler.Running();
                            return;
                    }
                }

                runHandler.NotReady();
            });
        }

        public override void Dispose()
        {
            runHandler.Dispose();
        }
    }
}

