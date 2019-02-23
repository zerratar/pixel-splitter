using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LiveSplit.PixelSplitter.Models;

namespace LiveSplit.PixelSplitter.Controls
{
    public partial class MatchActionEditorWindow : Form
    {
        public MatchActionEditorWindow(GameImageMatchAction actionToEdit = null)
        {
            InitializeComponent();

            this.MatchAction = actionToEdit != null
                ? actionToEdit.Clone()
                : new GameImageMatchAction(Guid.NewGuid(), GameImageMatchActionType.SplitOnMatch, null, new List<SplitComparisonImage>());

            foreach (var type in Enum.GetValues(typeof(GameImageMatchActionType)).Cast<GameImageMatchActionType>())
            {
                var index = this.actionTypes.Items.Add(type.ToString());
                if (type == this.MatchAction.Type)
                {
                    this.actionTypes.SelectedIndex = index;
                }
            }

            foreach (var image in MatchAction.ComparisonImages)
            {
                this.imageList.Items.Add(image.SourceImagePath);
            }

            this.splitName.Text = this.MatchAction.SplitName;
        }

        public GameImageMatchAction MatchAction { get; set; }

        private void actionTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.MatchAction.Type = (GameImageMatchActionType)Enum.GetValues(typeof(GameImageMatchActionType)).GetValue(actionTypes.SelectedIndex);
        }

        private void splitName_TextChanged(object sender, EventArgs e)
        {
            this.MatchAction.SplitName = splitName.Text;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Image files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp";
            ofd.Title = "Add image to use for comparison";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var fn in ofd.FileNames)
                {
                    this.imageList.Items.Add(fn);
                    this.MatchAction.ComparisonImages.Add(new SplitComparisonImage(fn));
                }
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (this.imageList.SelectedItems.Count == 0)
            {
                return;
            }

            if (MessageBox.Show("Do you really want to delete the selected comparison image(s)?",
                    "Delete comparison image(s)?", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            var enumerable = this.imageList.SelectedItems.Cast<string>().ToList();
            foreach (var item in enumerable)
            {
                this.imageList.Items.Remove(item);
                var i2 = this.MatchAction.ComparisonImages.FirstOrDefault(x => x.SourceImagePath == item);
                if (i2 != null)
                {
                    this.MatchAction.ComparisonImages.Remove(i2);
                }
            }
        }
    }
}
