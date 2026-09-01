namespace CollectionManager.Core.Types;

/// <summary>
/// A beatmap selected for merged .osz export, with an editable display name.
/// The display name is used as the Version field of the generated .osu file.
/// </summary>
public class MergedOszBeatmap
{
    private MergedOszBeatmap(Beatmap beatmap, bool isPlaceholder)
    {
        Beatmap = beatmap;
        IsPlaceholder = isPlaceholder;
    }

    public static MergedOszBeatmap FromBeatmap(Beatmap beatmap) => new(beatmap, false);

    /// <summary>
    /// Creates the built-in placeholder beatmap (based on osupack's "0.osu").
    /// It is always included in the export list and packed into the .osz as a
    /// "delete" difficulty. Cannot be removed.
    /// </summary>
    public static MergedOszBeatmap CreatePlaceholder() => new(null, true);

    public Beatmap Beatmap { get; }

    public bool IsPlaceholder { get; }

    /// <summary>
    /// Custom display name. Null means default naming is used.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Name shown in the export list UI.
    /// </summary>
    public string UiDisplayName => DisplayName ?? DefaultDisplayName;

    public string DefaultDisplayName => IsPlaceholder ? "delete" : $"{Beatmap?.Title} [{Beatmap?.Creator}]";

    /// <summary>
    /// Resolves the final display name used as the Version field.
    /// Placeholder naming follows the packed example: "delete [packCreator]".
    /// </summary>
    public string ResolveDisplayName(string packCreator)
    {
        if (!string.IsNullOrWhiteSpace(DisplayName))
        {
            return DisplayName;
        }

        return IsPlaceholder ? $"delete [{packCreator}]" : $"{Beatmap?.Title} [{Beatmap?.Creator}]";
    }
}