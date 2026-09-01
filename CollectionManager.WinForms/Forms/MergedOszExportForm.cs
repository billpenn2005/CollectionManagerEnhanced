namespace GuiComponents.Forms;

using CollectionManager.Common.Interfaces.Forms;
using CollectionManager.Core.Types;
using CollectionManager.WinForms.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public partial class MergedOszExportForm : BaseForm, IMergedOszExportForm
{
    private const string BeatmapsFormat = "CM.SourceBeatmaps";
    private const string ExportItemsFormat = "CM.ExportItems";

    private TextBox _renameTextBox;

    public MergedOszExportForm()
    {
        InitializeComponent();

        comboBox_collection.SelectedIndexChanged += (_, _) => SelectedCollectionChanged?.Invoke(this, EventArgs.Empty);
        button_add.Click += (_, _) => MoveToExportClicked?.Invoke(this, EventArgs.Empty);
        button_remove.Click += (_, _) => MoveBackClicked?.Invoke(this, EventArgs.Empty);
        button_moveUp.Click += (_, _) => MoveUpClicked?.Invoke(this, EventArgs.Empty);
        button_moveDown.Click += (_, _) => MoveDownClicked?.Invoke(this, EventArgs.Empty);
        button_browseOutputDirectory.Click += (_, _) => BrowseOutputDirectory();
        button_export.Click += (_, _) => ExportClicked?.Invoke(this, EventArgs.Empty);
        button_close.Click += (_, _) => Close();

        listView_source.ItemDrag += ListView_source_ItemDrag;
        listView_source.DragEnter += ListView_source_DragEnter;
        listView_source.DragDrop += ListView_source_DragDrop;
        listView_export.ItemDrag += ListView_export_ItemDrag;
        listView_export.DragEnter += ListView_export_DragEnter;
        listView_export.DragDrop += ListView_export_DragDrop;
        listView_export.DoubleClick += ListView_export_DoubleClick;
    }

    public event EventHandler SelectedCollectionChanged;
    public event EventHandler MoveToExportClicked;
    public event EventHandler MoveBackClicked;
    public event EventHandler MoveUpClicked;
    public event EventHandler MoveDownClicked;
    public event EventHandler<MergedOszRenameRequestEventArgs> RenameRequested;
    public event EventHandler ExportClicked;
    public event EventHandler<Beatmaps> BeatmapsDroppedToExport;
    public event EventHandler<IReadOnlyList<MergedOszBeatmap>> ExportItemsDroppedBack;

    public IOsuCollection SelectedCollection => (comboBox_collection.SelectedItem as CollectionItem)?.Collection;

    public Beatmaps SelectedSourceBeatmaps
        => new(listView_source.SelectedItems.Cast<ListViewItem>().Select(item => (Beatmap)item.Tag));

    public IReadOnlyList<MergedOszBeatmap> SelectedExportItems
        => listView_export.SelectedItems.Cast<ListViewItem>()
            .Select(item => (MergedOszBeatmap)item.Tag)
            .Where(item => !item.IsPlaceholder)
            .ToList();

    public string PackName => textBox_packName.Text;

    public string Creator => textBox_creator.Text;

    public string ExtraTags => textBox_extraTags.Text;

    public string OutputDirectory
    {
        get => textBox_outputDirectory.Text;
        set => textBox_outputDirectory.Text = value;
    }

    public void SetCollections(OsuCollections collections)
    {
        comboBox_collection.Items.Clear();

        foreach (IOsuCollection collection in collections)
        {
            _ = comboBox_collection.Items.Add(new CollectionItem(collection));
        }

        if (comboBox_collection.Items.Count is not 0)
        {
            comboBox_collection.SelectedIndex = 0;
        }
    }

    public void SetSourceBeatmaps(Beatmaps beatmaps)
    {
        listView_source.BeginUpdate();
        listView_source.Items.Clear();

        if (beatmaps is not null)
        {
            foreach (Beatmap beatmap in beatmaps)
            {
                ListViewItem item = new(beatmap.ToString())
                {
                    SubItems = { beatmap.DiffName, beatmap.PlayMode.ToString() },
                    Tag = beatmap,
                };

                if (beatmap is BeatmapExtension extension && extension.LocalBeatmapMissing)
                {
                    item.ForeColor = Color.Gray;
                }

                _ = listView_source.Items.Add(item);
            }
        }

        listView_source.EndUpdate();
    }

    public void SetExportItems(IReadOnlyList<MergedOszBeatmap> items)
    {
        listView_export.BeginUpdate();
        listView_export.Items.Clear();

        foreach (MergedOszBeatmap item in items)
        {
            string sourceText = item.IsPlaceholder
                ? "(built-in placeholder)"
                : $"{item.Beatmap.Artist} - {item.Beatmap.Title} [{item.Beatmap.DiffName}]";

            ListViewItem listItem = new(item.UiDisplayName)
            {
                SubItems = { sourceText },
                Tag = item,
            };

            if (item.IsPlaceholder)
            {
                listItem.ForeColor = Color.DimGray;
            }

            _ = listView_export.Items.Add(listItem);
        }

        listView_export.EndUpdate();
    }

    private void BrowseOutputDirectory()
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Select directory for the exported .osz file",
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            OutputDirectory = dialog.SelectedPath;
        }
    }

    private void ListView_source_ItemDrag(object sender, ItemDragEventArgs e)
    {
        List<Beatmap> beatmaps = listView_source.SelectedItems.Cast<ListViewItem>()
            .Select(item => item.Tag)
            .OfType<Beatmap>()
            .ToList();

        if (beatmaps.Count is 0)
        {
            return;
        }

        _ = listView_source.DoDragDrop(new DataObject(BeatmapsFormat, beatmaps), DragDropEffects.Copy);
    }

    private void ListView_export_ItemDrag(object sender, ItemDragEventArgs e)
    {
        List<MergedOszBeatmap> items = listView_export.SelectedItems.Cast<ListViewItem>()
            .Select(item => item.Tag)
            .OfType<MergedOszBeatmap>()
            .Where(item => !item.IsPlaceholder)
            .ToList();

        if (items.Count is 0)
        {
            return;
        }

        _ = listView_export.DoDragDrop(new DataObject(ExportItemsFormat, items), DragDropEffects.Copy);
    }

    private void ListView_source_DragEnter(object sender, DragEventArgs e)
        => e.Effect = e.Data.GetDataPresent(ExportItemsFormat) ? DragDropEffects.Copy : DragDropEffects.None;

    private void ListView_export_DragEnter(object sender, DragEventArgs e)
        => e.Effect = e.Data.GetDataPresent(BeatmapsFormat) ? DragDropEffects.Copy : DragDropEffects.None;

    private void ListView_source_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(ExportItemsFormat) && e.Data.GetData(ExportItemsFormat) is IReadOnlyList<MergedOszBeatmap> items)
        {
            ExportItemsDroppedBack?.Invoke(this, items);
        }
    }

    private void ListView_export_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(BeatmapsFormat) && e.Data.GetData(BeatmapsFormat) is List<Beatmap> beatmaps)
        {
            BeatmapsDroppedToExport?.Invoke(this, new Beatmaps(beatmaps));
        }
    }

    private void ListView_export_DoubleClick(object sender, EventArgs e)
    {
        Point cursorLocation = listView_export.PointToClient(Cursor.Position);
        ListViewItem item = listView_export.GetItemAt(cursorLocation.X, cursorLocation.Y);
        if (item is not null)
        {
            BeginRename(item);
        }
    }

    private void BeginRename(ListViewItem item)
    {
        EndRename(false);

        Rectangle itemBounds = item.GetBounds(ItemBoundsPortion.ItemOnly);
        Rectangle cellBounds = item.SubItems[0].Bounds;

        _renameTextBox = new TextBox
        {
            Bounds = new Rectangle(cellBounds.X, itemBounds.Y, Math.Max(100, itemBounds.Width), itemBounds.Height),
            Text = item.Text,
            BorderStyle = BorderStyle.FixedSingle,
            Tag = item,
        };

        listView_export.Controls.Add(_renameTextBox);
        _renameTextBox.BringToFront();
        _renameTextBox.Focus();
        _renameTextBox.SelectAll();

        _renameTextBox.KeyDown += (_, args) =>
        {
            if (args.KeyCode == Keys.Enter)
            {
                EndRename(true);
                args.SuppressKeyPress = true;
            }
            else if (args.KeyCode == Keys.Escape)
            {
                EndRename(false);
                args.SuppressKeyPress = true;
            }
        };

        _renameTextBox.LostFocus += (_, _) => EndRename(true);
    }

    private void EndRename(bool commit)
    {
        if (_renameTextBox is null)
        {
            return;
        }

        TextBox textBox = _renameTextBox;
        _renameTextBox = null;
        ListViewItem item = textBox.Tag as ListViewItem;
        string newName = textBox.Text;
        textBox.Dispose();

        if (!commit || item is null)
        {
            return;
        }

        int index = listView_export.Items.IndexOf(item);

        if (index >= 0)
        {
            RenameRequested?.Invoke(this, new MergedOszRenameRequestEventArgs(index, newName));
        }
    }

    private sealed class CollectionItem
    {
        public CollectionItem(IOsuCollection collection)
        {
            Collection = collection;
        }

        public IOsuCollection Collection { get; }

        public override string ToString() => $"{Collection.Name} ({Collection.NumberOfBeatmaps})";
    }
}