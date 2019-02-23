using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using AForge.Video.DirectShow;
using LiveSplit.Model;
using LiveSplit.PixelSplitter.Models;
using LiveSplit.PixelSplitter.Providers;
using LiveSplit.PixelSplitter.Repositories;
using LiveSplit.UI;
using Newtonsoft.Json;

namespace LiveSplit.PixelSplitter.Controls
{
    public partial class ComponentSettings : UserControl
    {
        private readonly LiveSplitState state;
        private readonly IGameImageProvider gameImageSourceProvider;
        private PixelSplitterSettings settings;
        private IActionRepositoryProvider repoProvider;
        private IActionRepository repo;
        public ComponentSettings(
            LiveSplitState state,
            IActionRepositoryProvider repoProvider,
            IGameImageProvider gameImageSourceProvider)
        {
            this.state = state;
            this.repoProvider = repoProvider;
            this.gameImageSourceProvider = gameImageSourceProvider;
            InitializeComponent();
            
            this.settings = new PixelSplitterSettings("OBS-Camera", 0.97f, new List<RectangleF>());
            this.Load += OnLoad;
            this.Dock = DockStyle.Fill;
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            this.repo = this.repoProvider.Get(this.state.Run.GameName, this.state.Run.CategoryName);
            this.actionList.Items.Clear();

            foreach (FilterInfo device in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                this.captureDeviceList.Items.Add(new CaptureDevice(device));
            }

            foreach (var item in repo.All())
            {
                this.actionList.Items.Add(Map(item));
            }

            this.UpdateControlValues();
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement settings_node = document.CreateElement("Settings");

            settings_node.AppendChild(SettingsHelper.ToElement(document, "Version", "1.0"));
            settings_node.AppendChild(SettingsHelper.ToElement(document, "Match", this.settings.ImageMatchValuePercent));
            settings_node.AppendChild(SettingsHelper.ToElement(document, "Mask", JsonConvert.SerializeObject(this.settings.ImageMatchMask)));
            settings_node.AppendChild(SettingsHelper.ToElement(document, "CaptureDevice", this.settings.CaptureDevice));

            return settings_node;
        }

        public void SetSettings(XmlNode settings)
        {
            this.settings.ImageMatchValuePercent = float.Parse(settings["Match"].InnerText);
            this.settings.CaptureDevice = settings["CaptureDevice"].InnerText;
            this.settings.ImageMatchMask = JsonConvert.DeserializeObject<List<RectangleF>>(settings["Mask"].InnerText);

            UpdateControlValues();
        }

        private void UpdateControlValues()
        {
            this.captureDeviceList.SelectedItem = this.captureDeviceList.Items.Cast<CaptureDevice>()
                .FirstOrDefault(x => x.Name == this.settings.CaptureDevice);
            minMatchPercent.Value = (int)(this.settings.ImageMatchValuePercent * 100);
            lblMinMatchPercent.Text = $"{minMatchPercent.Value}%";
        }

        public PixelSplitterSettings GetSplitterSettings()
        {
            return settings;
        }

        private void ComponentSettings_Load(object sender, EventArgs e)
        {
        }

        private void minMatchPercent_Scroll(object sender, EventArgs e)
        {
            settings.ImageMatchValuePercent = minMatchPercent.Value / 100f;
            lblMinMatchPercent.Text = $"{minMatchPercent.Value}%";
        }

        private void captureDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (captureDeviceList.SelectedItem is CaptureDevice device)
            {
                this.settings.CaptureDevice = device.Name;
            }
        }

        private void linkMaskEditor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var gameImageSource = this.gameImageSourceProvider.Get(this.settings.CaptureDevice);
            var editor = new MaskEditorWindow(gameImageSource, this.settings.ImageMatchMask);
            editor.ShowDialog(); // we don't have a dialog result on this one. just let the user close it. 
            this.settings.ImageMatchMask = editor.Masks;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var editor = new MatchActionEditorWindow();
            if (editor.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var action = editor.MatchAction;
            this.actionList.Items.Add(Map(action));
            repo.Store(action);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (this.actionList.SelectedItems.Count == 0)
            {
                return;
            }

            var msg = this.actionList.SelectedItems.Count == 1
                ? "Are you sure you want to delete this action?"
                : "Are you sure you want to delete these actions?";

            var title = this.actionList.SelectedItems.Count == 1
                ? "Delete match action?"
                : "Delete these actions?";

            if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            foreach (var item in this.actionList.SelectedItems.Cast<ListViewItem>().ToList())
            {
                repo.Remove(item.Tag as GameImageMatchAction);
                item.Remove();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (this.actionList.SelectedItems.Count == 0)
            {
                return;
            }

            var matchAction = this.actionList.SelectedItems[0].Tag as GameImageMatchAction;
            var editor = new MatchActionEditorWindow(matchAction);
            if (editor.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var action = editor.MatchAction;
            var item = this.actionList.SelectedItems[0];
            item.SubItems[0].Text = action.Type.ToString();
            item.SubItems[1].Text = String.Join(",", action.ComparisonImages.Select(x => new FileInfo(x.SourceImagePath).Name));
            item.SubItems[2].Text = action.SplitName;
            item.Tag = action;
            repo.Store(action);
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            foreach (var item in state.Run)
            {
                var action = new GameImageMatchAction();
                action.SplitName = item.Name;
                action.Type = GameImageMatchActionType.SplitOnMatch;

                var currentDirectory = System.IO.Directory.GetCurrentDirectory();
                var sourceImagePath = System.IO.Path.Combine(currentDirectory, JsonBasedActionRepositoryProvider.RepositoriesFolder, this.state.Run.GameName, item.Name + ".png");
                var splitComparisonImage = new SplitComparisonImage(sourceImagePath);
                action.ComparisonImages.Add(splitComparisonImage);
                this.actionList.Items.Add(Map(action));
                repo.Store(action);
            }
        }

        private static ListViewItem Map(GameImageMatchAction action)
        {
            return new ListViewItem(new[]
                {
                    action.Type.ToString(),
                    String.Join(",", action.ComparisonImages.Select(x => new FileInfo(x.SourceImagePath).Name)),
                    action.SplitName
                })
            { Tag = action };
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (this.actionList.SelectedItems.Count != 1)
            {
                return;
            }
            var index = this.actionList.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            var toSwitch = this.actionList.Items[index - 1];
            this.actionList.Items.Remove(toSwitch);
            this.actionList.Items.Insert(index, toSwitch);
            this.actionList.SelectedIndices.Clear();
            this.actionList.SelectedIndices.Add(index);
            this.repo.Move(index - 1, index);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (this.actionList.SelectedItems.Count != 1)
            {
                return;
            }
            var index = this.actionList.SelectedIndices[0];
            if (index == this.actionList.Items.Count - 1)
            {
                return;
            }

            var toSwitch = this.actionList.Items[index + 1];
            this.actionList.Items.Remove(toSwitch);
            this.actionList.Items.Insert(index - 1, toSwitch);
            this.actionList.SelectedIndices.Clear();
            this.actionList.SelectedIndices.Add(index);
            this.repo.Move(index + 1, index - 1);
        }
    }
}
