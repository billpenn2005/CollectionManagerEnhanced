namespace GuiComponents.Forms;

using CollectionManager.Common.Interfaces.Controls;
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

    private TextBox _editTextBox;

    public MergedOszExportForm()
    {
        InitializeComponent();

        // InsertionMark (drag&drop reorder indicator) only renders when the ListView has an image list.
        listView_export.SmallImageList = new ImageList
        {
            ImageSize = new Size(1, 1),
            ColorDepth = ColorDepth.Depth32Bit,
            TransparentColor = Color.Transparent,
        };

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
        listView_source.DoubleClick += ListView_source_DoubleClick;
        listView_export.ItemDrag += ListView_export_ItemDrag;
        listView_export.DragEnter += ListView_export_DragEnter;
        listView_export.DragOver += ListView_export_DragOver;
        listView_export.DragLeave += ListView_export_DragLeave;
        listView_export.DragDrop += ListView_export_DragDrop;
        listView_export.DoubleClick += ListView_export_DoubleClick;
        listView_export.SelectedIndexChanged += (_, _) => ExportSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler SelectedCollectionChanged;
    public event EventHandler MoveToExportClicked;
    public event EventHandler MoveBackClicked;
    public event EventHandler MoveUpClicked;
    public event EventHandler MoveDownClicked;
    public event EventHandler<MergedOszRenameRequestEventArgs> RenameRequested;
    public event EventHandler ExportClicked;
    public event EventHandler ExportSelectionChanged;
    public event EventHandler<Beatmaps> BeatmapsDroppedToExport;
    public event EventHandler<IReadOnlyList<MergedOszBeatmap>> ExportItemsDroppedBack;
    public event EventHandler<MergedOszReorderEventArgs> ReorderRequested;

    public IOsuCollection SelectedCollection => (comboBox_collection.SelectedItem as CollectionItem)?.Collection;

    public Beatmaps SelectedSourceBeatmaps
        => new(listView_source.SelectedItems.Cast<ListViewItem>().Select(item => (Beatmap)item.Tag));

    public IReadOnlyList<MergedOszBeatmap> SelectedExportItems
        => listView_export.SelectedItems.Cast<ListViewItem>()
            .Select(item => (MergedOszBeatmap)item.Tag)
            .Where(item => !item.IsPlaceholder)
            .ToList();

    /// <summary>Beatmap preview panel (same control as the main window's Map tab).</summary>
    public ICombinedBeatmapPreviewView CombinedBeatmapPreviewView => combinedBeatmapPreviewView1;

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

    public void SetBbcodeText(string text) => textBox_bbcode.Text = text;

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

        _ = listView_export.DoDragDrop(new DataObject(ExportItemsFormat, items), DragDropEffects.Move);
    }

    private void ListView_source_DragEnter(object sender, DragEventArgs e)
        => e.Effect = e.Data.GetDataPresent(ExportItemsFormat) ? DragDropEffects.Copy : DragDropEffects.None;

    private void ListView_export_DragEnter(object sender, DragEventArgs e)
        => e.Effect = e.Data.GetDataPresent(BeatmapsFormat) || e.Data.GetDataPresent(ExportItemsFormat) ? DragDropEffects.Move : DragDropEffects.None;

    private void ListView_export_DragOver(object sender, DragEventArgs e)
    {
        bool reorder = e.Data.GetDataPresent(ExportItemsFormat);
        bool add = e.Data.GetDataPresent(BeatmapsFormat);

        if (!reorder && !add)
        {
            e.Effect = DragDropEffects.None;
            return;
        }

        e.Effect = reorder ? DragDropEffects.Move : DragDropEffects.Copy;

        if (reorder && listView_export.Items.Count > 0)
        {
            Point cursorLocation = listView_export.PointToClient(new Point(e.X, e.Y));
            int nearest = listView_export.InsertionMark.NearestIndex(cursorLocation);
            nearest = Math.Max(0, Math.Min(nearest, listView_export.Items.Count - 1));

            ListViewItem targetItem = listView_export.Items[nearest];
            Rectangle bounds = targetItem.GetBounds(ItemBoundsPortion.Entire);
            bool appearsAfter = cursorLocation.Y > bounds.Top + bounds.Height / 2;

            listView_export.InsertionMark.Index = nearest;
            listView_export.InsertionMark.AppearsAfterItem = appearsAfter;
        }
    }

    private void ListView_export_DragLeave(object sender, EventArgs e)
        => listView_export.InsertionMark.Index = -1;

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
        else if (e.Data.GetDataPresent(ExportItemsFormat) && e.Data.GetData(ExportItemsFormat) is IReadOnlyList<MergedOszBeatmap> items)
        {
            int targetIndex = GetDropTargetIndex();
            listView_export.InsertionMark.Index = -1;
            ReorderRequested?.Invoke(this, new MergedOszReorderEventArgs(items, targetIndex));
        }
    }

    private int GetDropTargetIndex()
    {
        int index = listView_export.InsertionMark.Index;
        int targetIndex = listView_export.InsertionMark.AppearsAfterItem ? index + 1 : index;
        return Math.Clamp(targetIndex, 1, listView_export.Items.Count);
    }

    private void ListView_source_DoubleClick(object sender, EventArgs e)
    {
        Point cursorLocation = listView_source.PointToClient(Cursor.Position);
        ListViewHitTestInfo hit = listView_source.HitTest(cursorLocation);

        if (hit.Item is not null)
        {
            BeginInlineEdit(hit.Item, GetColumnIndex(listView_source, hit, cursorLocation), readOnly: true);
        }
    }

    private void ListView_export_DoubleClick(object sender, EventArgs e)
    {
        Point cursorLocation = listView_export.PointToClient(Cursor.Position);
        ListViewHitTestInfo hit = listView_export.HitTest(cursorLocation);

        if (hit.Item is not null)
        {
            int columnIndex = GetColumnIndex(listView_export, hit, cursorLocation);

            // Only the first (name) column is editable; other columns open a read-only copy box.
            BeginInlineEdit(hit.Item, columnIndex, readOnly: columnIndex > 0);
        }
    }

    private static int GetColumnIndex(ListView listView, ListViewHitTestInfo hit, Point point)
    {
        if (hit.SubItem is not null)
        {
            for (int i = 0; i < hit.Item.SubItems.Count; i++)
            {
                if (ReferenceEquals(hit.Item.SubItems[i], hit.SubItem))
                {
                    return i;
                }
            }

            return 0;
        }

        // Fallback for clicks on column borders / empty area: locate by X against sub-item bounds.
        int x = point.X;
        ListViewItem.ListViewSubItemCollection subItems = hit.Item.SubItems;

        for (int i = 0; i < listView.Columns.Count && i < subItems.Count; i++)
        {
            Rectangle bounds = subItems[i].Bounds;

            if (x < bounds.Left)
            {
                break;
            }

            if (x <= bounds.Right)
            {
                return i;
            }
        }

        return 0;
    }

    private void BeginInlineEdit(ListViewItem item, int subItemIndex, bool readOnly)
    {
        EndInlineEdit(false);

        subItemIndex = Math.Max(0, Math.Min(subItemIndex, item.SubItems.Count - 1));

        Rectangle itemBounds = item.GetBounds(ItemBoundsPortion.ItemOnly);
        Rectangle cellBounds = item.SubItems[subItemIndex].Bounds;

        _editTextBox = new TextBox
        {
            Bounds = new Rectangle(cellBounds.X, itemBounds.Y, Math.Max(100, itemBounds.Width), itemBounds.Height),
            Text = item.SubItems[subItemIndex].Text,
            BorderStyle = BorderStyle.FixedSingle,
            ReadOnly = readOnly,
            Tag = item,
        };

        ListView owner = readOnly ? listView_source : listView_export;
        owner.Controls.Add(_editTextBox);
        _editTextBox.BringToFront();
        _editTextBox.Focus();
        _editTextBox.SelectAll();

        _editTextBox.KeyDown += (_, args) =>
        {
            if (args.KeyCode == Keys.Enter)
            {
                EndInlineEdit(!readOnly);
                args.SuppressKeyPress = true;
            }
            else if (args.KeyCode == Keys.Escape)
            {
                EndInlineEdit(false);
                args.SuppressKeyPress = true;
            }
        };

        _editTextBox.LostFocus += (_, _) => EndInlineEdit(!readOnly);
    }

    private void EndInlineEdit(bool commit)
    {
        if (_editTextBox is null)
        {
            return;
        }

        TextBox textBox = _editTextBox;
        _editTextBox = null;
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