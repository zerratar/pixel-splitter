using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LiveSplit.Model.Input;
using LiveSplit.PixelSplitter.Comparer;
using LiveSplit.PixelSplitter.Models;
using LiveSplit.PixelSplitter.Providers;
using LiveSplit.PixelSplitter.Repositories;

namespace LiveSplit.PixelSplitter
{
    internal class PixelSplitterRunHandler : IRunHandler
    {
        private readonly IActionMatchComparer actionMatcher;
        private readonly IGameImageProvider gameImageProvider;
        private readonly IPixelSplitterSettingsProvider settingsProvider;
        private readonly LiveSplitController controller;
        private readonly IActionRepositoryProvider actionRepositoryProvider;

        private IGameImageSource gameImageSource;
        private IActionRepository actionRepository;
        private string lastSplitName;
        private int screenshotId = 1;
        private bool tempPauseActive;

        public PixelSplitterRunHandler(
            LiveSplitController controller,
            IActionRepositoryProvider actionRepositoryProvider,
            IActionMatchComparer actionMatcher,
            IGameImageProvider gameImageProvider,
            IPixelSplitterSettingsProvider settingsProvider)
        {
            this.controller = controller;
            this.actionRepositoryProvider = actionRepositoryProvider;
            this.actionMatcher = actionMatcher;
            this.gameImageProvider = gameImageProvider;
            this.settingsProvider = settingsProvider;
        }

        public bool IsReady => gameImageSource != null && gameImageSource.Available && actionRepository != null;

        public void Init(string captureSource)
        {
            this.gameImageSource = this.gameImageProvider.Get(captureSource);

            var kb = new Keyboard();
            kb.OnKeyUp += KbOnOnKeyUp;

            EnsureRepository();
        }

        public void NotReady()
        {
        }

        public void NotRunning()
        {
            EnsureRepository();

            try
            {
                this.lastSplitName = this.controller.CurrentSplitName;
                if (!gameImageSource.HasNewFrame)
                {
                    return;
                }

                var settings = settingsProvider.Get();
                var image = gameImageSource.GetMaskedFrame(settings);
                if (GetAndMatch(image, GameImageMatchActionType.StartOnMatch))
                {
                    controller.Start();
                }
            }
            finally
            {
                gameImageSource.Next();
            }
        }

        public void Ended()
        {
            if (!gameImageSource.HasNewFrame)
            {
                return;
            }

            gameImageSource.Next();
        }

        public void Paused()
        {
            try
            {
                if (!gameImageSource.HasNewFrame)
                {
                    return;
                }

                var settings = settingsProvider.Get();
                var image = gameImageSource.GetMaskedFrame(settings);
                if (GetAndMatch(image, GameImageMatchActionType.EndOnMatch))
                {
                    controller.End();
                    return;
                }

                if (GetAndMatch(image, GameImageMatchActionType.AbortOnMatch))
                {
                    controller.Abort();
                    return;
                }

                if (GetAndMatch(image, GameImageMatchActionType.ResumeOnMatch))
                {
                    controller.Resume();
                    return;
                }

                if (tempPauseActive && actionRepository.TryGet(GameImageMatchActionType.PausedWhileMatch, out var keepPaused))
                {
                    if (!actionMatcher.AnyMatch(image, keepPaused))
                    {
                        controller.Resume();
                        this.tempPauseActive = false;
                    }
                }
            }
            finally
            {
                gameImageSource.Next();
            }
        }

        public void Running()
        {
            if (!gameImageSource.HasNewFrame)
            {
                return;
            }

            try
            {
                var settings = settingsProvider.Get();
                var image = gameImageSource.GetMaskedFrame(settings);
                if (GetAndMatch(image, GameImageMatchActionType.EndOnMatch))
                {
                    controller.End();
                    return;
                }

                if (GetAndMatch(image, GameImageMatchActionType.AbortOnMatch))
                {
                    controller.Abort();
                    return;
                }

                if (GetAndMatch(image, GameImageMatchActionType.PauseOnMatch))
                {
                    controller.Pause();
                    return;
                }

                if (GetAndMatch(image, GameImageMatchActionType.PausedWhileMatch))
                {
                    controller.Pause();
                    tempPauseActive = true;
                    return;
                }

                if (GetAndMatchSplit(image, GameImageMatchActionType.SkipOnMatch))
                {
                    controller.Skip();
                    return;
                }

                if (GetAndMatchSplit(image, GameImageMatchActionType.SplitAndPauseOnMatch))
                {
                    controller.Split();
                    controller.Pause();
                    return;
                }

                if (GetAndMatchSplit(image, GameImageMatchActionType.SplitOnMatch))
                {
                    controller.Split();
                    return;
                }

                SaveMatchImage(); // image

            }
            finally
            {
                gameImageSource.Next();
            }
        }

        public void Dispose()
        {
            gameImageSource?.Dispose();
        }

        private void KbOnOnKeyUp(object sender, KeyboardEventArgs keyboardEvent)
        {
            if (keyboardEvent.Key == Keys.Subtract)
            {
                this.controller.UndoSplit();
            }
            else if (keyboardEvent.Key == Keys.Add)
            {
                AddMatchImage(this.controller.CurrentSplitName);
            }
            else if (keyboardEvent.Key == Keys.NumPad7)
            {
                var folder = System.IO.Path.Combine(
                    JsonBasedActionRepositoryProvider.RepositoriesFolder,
                    this.controller.GameName,
                    "screenshots");

                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                //var files = new HashSet<string>(System.IO.Directory.GetFiles(folder, "*.png").Select(x => new FileInfo(x).Name));
                //var targetFilename = screenshotId + ".png";

                //while (files.Contains(targetFilename))
                //    targetFilename = (++screenshotId) + ".png";

                gameImageSource.SaveFrame(Path.Combine(folder, GetNextFileName(folder, ".png")));
            }
        }

        private void AddMatchImage(string splitName)
        {
            var match = this.actionRepository.Find(x => x.SplitName == splitName);
            if (match == null)
            {
                return;
            }

            var folder = System.IO.Path.Combine(JsonBasedActionRepositoryProvider.RepositoriesFolder,
                this.controller.GameName, this.controller.CategoryName);
            var path = System.IO.Path.Combine(folder, GetNextFileName(folder, ".png", splitName));
            gameImageSource.SaveFrame(path);
            match.ComparisonImages.Add(new SplitComparisonImage(path));
            this.actionRepository.Save();
        }

        private void SaveMatchImage() // IMaskedGameImage image
        {
            // take a screenshot if we have made a split manually and use that as a comparison image
            // if we don't have one.
            if (this.lastSplitName != this.controller.CurrentSplitName)
            {
                if (this.actionRepository.TryGet(GameImageMatchActionType.SplitOnMatch, out var matches))
                {
                    var match = matches.FirstOrDefault(x => x.SplitName == this.lastSplitName);
                    if (match == null) return;

                    var nonExistingImage =
                        match.ComparisonImages.FirstOrDefault(x => !System.IO.File.Exists(x.SourceImagePath));
                    if (nonExistingImage != null)
                    {
                        gameImageSource.SaveFrame(nonExistingImage.SourceImagePath);
                        //image.Save(nonExistingImage.SourceImagePath);
                    }
                }
                this.lastSplitName = this.controller.CurrentSplitName;
            }
        }

        private void EnsureRepository()
        {
            if (this.actionRepository == null || this.actionRepository.Identifier != controller.Identifier)
            {
                this.actionRepository = this.actionRepositoryProvider.Get(controller.GameName, controller.CategoryName);
            }
        }
        private bool GetAndMatchSplit(IMaskedGameImage gameImage, GameImageMatchActionType type)
        {
            return GetAndMatch(gameImage, type, this.controller.CurrentSplitName);
        }

        private bool GetAndMatch(IMaskedGameImage gameImage, GameImageMatchActionType type, string splitName = null)
        {
            if (actionRepository.TryGet(type, out var split))
            {
                if (!string.IsNullOrEmpty(splitName))
                {
                    split = split.Where(x => x.SplitName == splitName).ToList();
                    if (split.Count == 0)
                    {
                        return false;
                    }
                }
                if (actionMatcher.AnyMatch(gameImage, split))
                {
                    return true;
                }
            }
            return false;
        }

        private string GetNextFileName(string path, string ext, string prefix = null)
        {
            var num = 1;
            var files = new HashSet<string>(System.IO.Directory.GetFiles(path, "*" + ext).Select(x => new FileInfo(x).Name));
            var targetFilename = prefix + num + ext;
            while (files.Contains(targetFilename))
                targetFilename = prefix + (++num) + ext;
            return targetFilename;
        }

    }
}