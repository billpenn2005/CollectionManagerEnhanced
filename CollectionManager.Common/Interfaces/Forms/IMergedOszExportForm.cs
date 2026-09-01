namespace CollectionManager.Common.Interfaces.Forms;

using CollectionManager.Core.Types;
using System;
using System.Collections.Generic;

public interface IMergedOszExportForm : IForm
{
    /// <summary>Collection currently selected in the dropdown.</summary>
    IOsuCollection SelectedCollection { get; }

    /// <summary>Beatmaps currently selected in the source list.</summary>
    Beatmaps SelectedSourceBeatmaps { get; }

    /// <summary>Items currently selected in the export list (placeholder excluded).</summary>
    IReadOnlyList<MergedOszBeatmap> SelectedExportItems { get; }

    string PackName { get; }

    string Creator { get; }

    string ExtraTags { get; }

    string OutputDirectory { get; set; }

    void SetCollections(OsuCollections collections);

    void SetSourceBeatmaps(Beatmaps beatmaps);

    void SetExportItems(IReadOnlyList<MergedOszBeatmap> items);

    event EventHandler SelectedCollectionChanged;

    event EventHandler MoveToExportClicked;

    event EventHandler MoveBackClicked;

    event EventHandler MoveUpClicked;

    event EventHandler MoveDownClicked;

    event EventHandler<MergedOszRenameRequestEventArgs> RenameRequested;

    event EventHandler ExportClicked;

    /// <summary>Raised by the view when a drag&drop adds beatmaps to the export list.</summary>
    event EventHandler<Beatmaps> BeatmapsDroppedToExport;

    /// <summary>Raised by the view when a drag&drop removes items from the export list.</summary>
    event EventHandler<IReadOnlyList<MergedOszBeatmap>> ExportItemsDroppedBack;

    /// <summary>Raised by the view when export-list items are drag&dropped to reorder them within the list.</summary>
    event EventHandler<MergedOszReorderEventArgs> ReorderRequested;
}

public sealed class MergedOszRenameRequestEventArgs : EventArgs
{
    public MergedOszRenameRequestEventArgs(int index, string newName)
    {
        Index = index;
        NewName = newName;
    }

    public int Index { get; }

    public string NewName { get; }
}

public sealed class MergedOszReorderEventArgs : EventArgs
{
    public MergedOszReorderEventArgs(IReadOnlyList<MergedOszBeatmap> items, int targetIndex)
    {
        Items = items;
        TargetIndex = targetIndex;
    }

    /// <summary>The dragged items, in the order they appeared in the list.</summary>
    public IReadOnlyList<MergedOszBeatmap> Items { get; }

    /// <summary>Position in the full list (placeholder at index 0) where the items should be inserted.</summary>
    public int TargetIndex { get; }
}