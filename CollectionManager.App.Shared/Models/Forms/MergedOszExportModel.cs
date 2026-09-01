namespace CollectionManager.App.Shared.Models.Forms;

using CollectionManager.Core.Types;
using System.Collections.Generic;

public class MergedOszExportModel
{
    public OsuCollections Collections { get; set; }

    public IOsuCollection SelectedCollection { get; set; }

    public Beatmaps SourceBeatmaps { get; set; }

    /// <summary>
    /// Items to export. The placeholder (index 0) is always present and cannot be removed.
    /// </summary>
    public List<MergedOszBeatmap> ExportItems { get; } = [MergedOszBeatmap.CreatePlaceholder()];
}