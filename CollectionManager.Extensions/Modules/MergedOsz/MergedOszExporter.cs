namespace CollectionManager.Extensions.Modules.MergedOsz;

using CollectionManager.Core.Enums;
using CollectionManager.Core.Types;
using CollectionManager.Extensions.Utils;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// Packs multiple beatmaps into a single .osz archive (one difficulty per beatmap).
/// Naming follows the established pack format:
/// - .osu files: "Various Artists - {packName} ({creator}) [{displayName}].osu"
///   with Version set to the display name (default "Title [Creator]")
/// - Resources: "{index}.{extension}" (audio + background share the same index)
/// - Title = packName, Artist = "Various Artists", Creator = pack creator,
///   BeatmapID = 0, BeatmapSetID = -1, Tags = combined tags of all beatmaps.
/// </summary>
public class MergedOszExporter
{
    /// <summary>
    /// Placeholder beatmap template (mirrors osupack's "0.osu").
    /// Gets rewritten by the same rules as regular beatmaps and is packed
    /// into the .osz as a "delete [{creator}]" difficulty.
    /// </summary>
    private const string PlaceholderOsuTemplate = """
        osu file format v14

        [General]
        AudioFilename: .mp3
        AudioLeadIn: 0
        PreviewTime: 220869
        Countdown: 0
        SampleSet: None
        StackLeniency: 0.7
        Mode: 3
        LetterboxInBreaks: 0
        SpecialStyle: 0
        WidescreenStoryboard: 1

        [Editor]
        Bookmarks: 116521
        DistanceSpacing: 0.7
        BeatDivisor: 8
        GridSize: 4
        TimelineZoom: 2.799999

        [Metadata]
        Title:delete
        TitleUnicode:delete
        Artist:delete
        ArtistUnicode:delete
        Creator:billpenn
        Version:delete
        Source:
        Tags:
        BeatmapID:0
        BeatmapSetID:-1

        [Difficulty]
        HPDrainRate:8
        CircleSize:4
        OverallDifficulty:8
        ApproachRate:5
        SliderMultiplier:1.4
        SliderTickRate:1

        [Events]
        //Background and Video events
        0,0,"16.png",0,0
        //Break Periods
        2,215,99998799
        //Storyboard Layer 0 (Background)
        //Storyboard Layer 1 (Fail)
        //Storyboard Layer 2 (Pass)
        //Storyboard Layer 3 (Foreground)
        //Storyboard Layer 4 (Overlay)
        //Storyboard Sound Samples

        [TimingPoints]
        0,434.782608695652,4,2,0,15,1,0
        222602,-100,4,2,0,23,0,1
        278254,-100,4,2,0,15,0,0


        [HitObjects]
        192,192,15,5,0,0:0:0:0:
        64,192,99999999,5,0,0:0:0:0:
        """;

    private static readonly string[] _backgroundFileFormats = [".jpg", ".jpeg", ".png"];

    private readonly string _songsDirectory;
    private readonly string _saveDirectory;

    public MergedOszExporter(string songsDirectory, string saveDirectory)
    {
        _songsDirectory = songsDirectory;
        _saveDirectory = saveDirectory;
    }

    public readonly record struct FailedExport(MergedOszBeatmap Item, Exception Error);

    /// <summary>
    /// Exports all items into a single "{packName}.osz" in the save directory,
    /// along with a "bbcode.txt" song list. Returns items that failed to process.
    /// </summary>
    public IReadOnlyList<FailedExport> Export(IEnumerable<MergedOszBeatmap> items, string packName, string creator, string extraTags, IProgress<string> statusProgress = null, IProgress<int> percentageProgress = null, CancellationToken cancellationToken = default)
    {
        MergedOszBeatmap[] itemsArray = items?.ToArray() ?? [];

        if (itemsArray.Length is 0)
        {
            throw new InvalidOperationException("No beatmaps to export.");
        }

        if (string.IsNullOrWhiteSpace(packName))
        {
            throw new ArgumentException("Pack name is required.", nameof(packName));
        }

        if (string.IsNullOrWhiteSpace(creator))
        {
            throw new ArgumentException("Creator name is required.", nameof(creator));
        }

        string allTags = string.Join(' ', itemsArray
            .Where(item => !item.IsPlaceholder)
            .Select(item => item.Beatmap?.Tags)
            .Where(tags => !string.IsNullOrWhiteSpace(tags))
            .Append(extraTags))
            .Trim();

        string tempDirectory = Path.Combine(_saveDirectory, "tmp_pack");

        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, true);
        }

        Directory.CreateDirectory(tempDirectory);

        try
        {
            List<ProcessedEntry> entries = [];
            List<FailedExport> failedExports = [];

            for (int index = 0; index < itemsArray.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                MergedOszBeatmap item = itemsArray[index];
                statusProgress?.Report($"Processing beatmap {index + 1} of {itemsArray.Length}.{Environment.NewLine}\"{item.UiDisplayName}\"");

                try
                {
                    entries.Add(ProcessItem(item, index + 1, packName, creator, allTags, tempDirectory));
                }
                catch (Exception exception)
                {
                    failedExports.Add(new FailedExport(item, exception));
                }

                percentageProgress?.Report(Convert.ToInt32((double)(index + 1) / itemsArray.Length * 100));
            }

            cancellationToken.ThrowIfCancellationRequested();

            statusProgress?.Report("Writing osz archive...");

            string oszPath = Path.Combine(_saveDirectory, $"{SanitizeFileName(packName)}.osz");

            using (ZipArchive zip = ZipArchive.Create())
            {
                foreach (ProcessedEntry entry in entries)
                {
                    zip.AddEntry(entry.OsuFileName, entry.OsuFilePath);

                    foreach ((string zipName, string sourcePath) in entry.Resources)
                    {
                        zip.AddEntry(zipName, sourcePath);
                    }
                }

                using FileStream fileStream = new(oszPath, FileMode.Create, FileAccess.ReadWrite);
                zip.SaveTo(fileStream);
            }

            return failedExports;
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    private sealed class ProcessedEntry
    {
        public ProcessedEntry(string osuFileName, string osuFilePath, List<(string ZipName, string SourcePath)> resources)
        {
            OsuFileName = osuFileName;
            OsuFilePath = osuFilePath;
            Resources = resources;
        }

        public string OsuFileName { get; }

        public string OsuFilePath { get; }

        public List<(string ZipName, string SourcePath)> Resources { get; }
    }

    private ProcessedEntry ProcessItem(MergedOszBeatmap item, int index, string packName, string creator, string allTags, string tempDirectory)
    {
        Beatmap beatmap = item.Beatmap;
        string[] lines;

        if (item.IsPlaceholder)
        {
            lines = PlaceholderOsuTemplate.Split('\n');
        }
        else
        {
            string sourceOsuPath;

            if (beatmap is LazerBeatmap lazerBeatmap)
            {
                LazerFile osuFile = lazerBeatmap.SetFiles?.FirstOrDefault(f => f.FileName.EndsWith(".osu", StringComparison.OrdinalIgnoreCase));

                if (osuFile is null)
                {
                    throw new FileNotFoundException($"Lazer beatmap \"{beatmap}\" has no .osu file in its set.");
                }

                sourceOsuPath = Path.Combine(_songsDirectory, osuFile.RelativeRealmFilePath);
            }
            else
            {
                sourceOsuPath = beatmap.FullOsuFileLocation();
            }

            if (!File.Exists(sourceOsuPath))
            {
                throw new FileNotFoundException($"Beatmap file not found at \"{sourceOsuPath}\"");
            }

            lines = File.ReadAllLines(sourceOsuPath);
        }

        string displayName = item.ResolveDisplayName(creator);
        string audioExtension = item.IsPlaceholder ? "mp3" : Path.GetExtension(beatmap?.Mp3Name)?.TrimStart('.');

        if (string.IsNullOrEmpty(audioExtension))
        {
            audioExtension = "mp3";
        }

        // Rewrite the .osu content
        string backgroundFileName = null;
        bool backgroundReplaced = false;
        bool inEvents = false;
        List<string> outputLines = new(lines.Length);

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd('\r');

            if (line.Length > 0 && line[0] == '[' && line[^1] == ']')
            {
                inEvents = line.Equals("[Events]", StringComparison.OrdinalIgnoreCase);
                outputLines.Add(line);
                continue;
            }

            if (line.StartsWith("AudioFilename:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add($"AudioFilename:{index}.{audioExtension}");
                continue;
            }

            if (line.StartsWith("TitleUnicode:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add($"TitleUnicode:{packName}");
                continue;
            }

            if (line.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add($"Title:{packName}");
                continue;
            }

            if (line.StartsWith("ArtistUnicode:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add("ArtistUnicode:Various Artists");
                continue;
            }

            if (line.StartsWith("Artist:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add("Artist:Various Artists");
                continue;
            }

            if (line.StartsWith("Version:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add($"Version:{displayName}");
                continue;
            }

            if (line.StartsWith("BeatmapID:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add("BeatmapID:0");
                continue;
            }

            if (line.StartsWith("BeatmapSetID:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add("BeatmapSetID:-1");
                continue;
            }

            if (line.StartsWith("Creator:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add($"Creator:{creator}");
                continue;
            }

            if (line.StartsWith("Tags:", StringComparison.OrdinalIgnoreCase))
            {
                outputLines.Add($"Tags:{allTags}");
                continue;
            }

            if (inEvents
                && !backgroundReplaced
                && IsBackgroundLine(line, out string backgroundFileNameRaw))
            {
                string backgroundExtension = Path.GetExtension(backgroundFileNameRaw)?.TrimStart('.');
                string[] split = line.Split(',');
                outputLines.Add($"{split[0]},{split[1]},\"{index}.{backgroundExtension}\",{string.Join(',', split.Skip(3))}");
                backgroundFileName = backgroundFileNameRaw;
                backgroundReplaced = true;
                continue;
            }

            outputLines.Add(line);
        }

        // Copy resources (audio + background) as "{index}.{extension}"
        List<(string ZipName, string SourcePath)> resources = [];

        if (!item.IsPlaceholder)
        {
            string audioSourcePath = null;

            if (beatmap is LazerBeatmap lazerAudio && !string.IsNullOrEmpty(lazerAudio.AudioRelativeFilePath))
            {
                audioSourcePath = Path.Combine(_songsDirectory, lazerAudio.AudioRelativeFilePath);
            }
            else if (!string.IsNullOrEmpty(beatmap.Mp3Name))
            {
                audioSourcePath = Path.Combine(beatmap.BeatmapDirectory(_songsDirectory), beatmap.Mp3Name);
            }

            if (!string.IsNullOrWhiteSpace(audioSourcePath) && File.Exists(audioSourcePath))
            {
                resources.Add(($"{index}.{audioExtension}", audioSourcePath));
            }

            string backgroundSourcePath = null;

            if (beatmap is LazerBeatmap lazerBackground && !string.IsNullOrEmpty(lazerBackground.BackgroundRelativeFilePath))
            {
                backgroundSourcePath = Path.Combine(_songsDirectory, lazerBackground.BackgroundRelativeFilePath);
            }
            else if (!string.IsNullOrEmpty(backgroundFileName))
            {
                backgroundSourcePath = Path.Combine(beatmap.BeatmapDirectory(_songsDirectory), backgroundFileName);
            }

            if (!string.IsNullOrWhiteSpace(backgroundSourcePath) && File.Exists(backgroundSourcePath))
            {
                resources.Add(($"{index}.{Path.GetExtension(backgroundSourcePath)?.TrimStart('.')}", backgroundSourcePath));
            }
        }

        string osuFileName = $"{SanitizeFileName($"Various Artists - {packName} ({creator}) [{displayName}]")}.osu";
        string osuFilePath = Path.Combine(tempDirectory, osuFileName);

        File.WriteAllLines(osuFilePath, outputLines, Encoding.UTF8);

        return new ProcessedEntry(osuFileName, osuFilePath, resources);
    }

    private static bool IsBackgroundLine(string line, out string backgroundFileName)
    {
        backgroundFileName = null;

        if (!_backgroundFileFormats.Any(fileFormat => line.Contains(fileFormat, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        string[] split = line.Split(',');

        if (split.Length < 3)
        {
            return false;
        }

        backgroundFileName = split[2].Trim('"');
        return true;
    }

    /// <summary>
    /// Builds the [box=map list] song listing used for forum posts (osupack's bbcode.txt).
    /// The .osz export itself no longer writes this to disk; the UI shows it as live preview.
    /// </summary>
    public static string BuildBbcode(IEnumerable<MergedOszBeatmap> items)
    {
        StringBuilder stringBuilder = new();
        _ = stringBuilder.AppendLine("[box=map list]");

        foreach (MergedOszBeatmap item in items.Where(i => !i.IsPlaceholder))
        {
            Beatmap beatmap = item.Beatmap;
            string mode = beatmap.PlayMode switch
            {
                PlayMode.Taiko => "taiko",
                PlayMode.CatchTheBeat => "fruits",
                PlayMode.OsuMania => "mania",
                _ => "osu",
            };

            _ = stringBuilder.AppendLine($"[url=https://osu.ppy.sh/beatmapsets/{beatmap.MapSetId}#{mode}/{beatmap.MapId}]{beatmap.Title}[/url] by [url=https://osu.ppy.sh/users/{beatmap.Creator}]{beatmap.Creator}[/url]");
        }

        _ = stringBuilder.AppendLine("[/box]");
        return stringBuilder.ToString();
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalidChars.Contains(c) ? '_' : c));
    }
}