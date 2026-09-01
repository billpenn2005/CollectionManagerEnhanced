namespace CollectionManager.Extensions.Tests.Modules.MergedOsz;

using CollectionManager.Core.Enums;
using CollectionManager.Core.Types;
using CollectionManager.Extensions.Modules.MergedOsz;
using CollectionManager.Extensions.Utils;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Linq;
using Xunit;

public sealed class MergedOszExporterTests : IDisposable
{
    private const string PackName = "Test Pack";
    private const string PackCreator = "Packer";

    private readonly string _rootDirectory;
    private readonly string _songsDirectory;
    private readonly string _saveDirectory;

    public MergedOszExporterTests()
    {
        _rootDirectory = Path.Combine(Path.GetTempPath(), "MergedOszExporterTests_" + Guid.NewGuid().ToString("N"));
        _songsDirectory = Path.Combine(_rootDirectory, "Songs");
        _saveDirectory = Path.Combine(_rootDirectory, "Output");
        Directory.CreateDirectory(_songsDirectory);
        Directory.CreateDirectory(_saveDirectory);
        BeatmapUtils.OsuSongsDirectory = _songsDirectory;
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
        {
            Directory.Delete(_rootDirectory, true);
        }
    }

    [Fact]
    public void ExportShouldProducePackWithRewrittenOsuFilesAndNumberedResources()
    {
        CreateBeatmapFolder(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");
        CreateBeatmapFolder(2, "Song Two", "Creator Two", "audio2.ogg", "bg2.png", "song2");

        BeatmapExtension songOne = CreateBeatmap(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");
        BeatmapExtension songTwo = CreateBeatmap(2, "Song Two", "Creator Two", "audio2.ogg", "bg2.png", "song2");

        MergedOszExporter exporter = new(_songsDirectory, _saveDirectory);
        IReadOnlyList<MergedOszExporter.FailedExport> failed = exporter.Export(
            [MergedOszBeatmap.CreatePlaceholder(), MergedOszBeatmap.FromBeatmap(songOne), MergedOszBeatmap.FromBeatmap(songTwo)],
            PackName,
            PackCreator,
            "extraTag");

        Assert.Empty(failed);
        Assert.True(File.Exists(Path.Combine(_saveDirectory, $"{PackName}.osz")), "osz file should exist");

        string entryList = GetEntryList(Path.Combine(_saveDirectory, $"{PackName}.osz"));

        // Placeholder gets index 1, songs get 2 and 3
        Assert.Contains("Various Artists - Test Pack (Packer) [delete [Packer]].osu", entryList);
        Assert.Contains("Various Artists - Test Pack (Packer) [Song One [Creator One]].osu", entryList);
        Assert.Contains("Various Artists - Test Pack (Packer) [Song Two [Creator Two]].osu", entryList);
        Assert.Contains("2.mp3", entryList);
        Assert.Contains("2.jpg", entryList);
        Assert.Contains("3.ogg", entryList);
        Assert.Contains("3.png", entryList);
        // Placeholder has no resources
        Assert.DoesNotContain("\n1.", entryList);
    }

    [Fact]
    public void GeneratedOsuShouldHavePackMetadata()
    {
        CreateBeatmapFolder(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");
        BeatmapExtension songOne = CreateBeatmap(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");

        MergedOszExporter exporter = new(_songsDirectory, _saveDirectory);
        _ = exporter.Export([MergedOszBeatmap.CreatePlaceholder(), MergedOszBeatmap.FromBeatmap(songOne)], PackName, PackCreator, "extraTag");

        string oszPath = Path.Combine(_saveDirectory, $"{PackName}.osz");
        using IArchive archive = ZipArchive.Open(oszPath);
        IArchiveEntry entry = archive.Entries.First(e => e.Key.Contains("Song One", StringComparison.Ordinal));
        using StreamReader reader = new(entry.OpenEntryStream());
        string content = reader.ReadToEnd();

        Assert.Contains("Title:Test Pack", content);
        Assert.Contains("TitleUnicode:Test Pack", content);
        Assert.Contains("Artist:Various Artists", content);
        Assert.Contains("ArtistUnicode:Various Artists", content);
        Assert.Contains("Creator:Packer", content);
        Assert.Contains("Version:Song One [Creator One]", content);
        Assert.Contains("AudioFilename:2.mp3", content);
        Assert.Contains("BeatmapID:0", content);
        Assert.Contains("BeatmapSetID:-1", content);
        Assert.Contains("Tags:song1 extraTag", content);
        Assert.Contains("0,0,\"2.jpg\",0,0", content);
        Assert.Contains("Mode: 3", content);
    }

    [Fact]
    public void ExportShouldNotWriteBbcodeFile()
    {
        CreateBeatmapFolder(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");
        BeatmapExtension songOne = CreateBeatmap(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");

        MergedOszExporter exporter = new(_songsDirectory, _saveDirectory);
        _ = exporter.Export([MergedOszBeatmap.CreatePlaceholder(), MergedOszBeatmap.FromBeatmap(songOne)], PackName, PackCreator, "");

        Assert.False(File.Exists(Path.Combine(_saveDirectory, "bbcode.txt")), "bbcode.txt should no longer be written to disk");
    }

    [Fact]
    public void BuildBbcodeShouldCreateSongList()
    {
        BeatmapExtension songOne = CreateBeatmap(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");
        BeatmapExtension songTwo = CreateBeatmap(2, "Song Two", "Creator Two", "audio2.ogg", "bg2.png", "song2");

        string bbcode = MergedOszExporter.BuildBbcode(
        [
            MergedOszBeatmap.CreatePlaceholder(),
            MergedOszBeatmap.FromBeatmap(songOne),
            MergedOszBeatmap.FromBeatmap(songTwo),
        ]);

        Assert.Contains("[box=map list]", bbcode);
        Assert.Contains("[/box]", bbcode);
        Assert.Contains("https://osu.ppy.sh/beatmapsets/100#mania/200", bbcode);
        Assert.Contains("https://osu.ppy.sh/beatmapsets/200#mania/400", bbcode);
        // Placeholder should not appear in the song list
        Assert.DoesNotContain("delete", bbcode);
    }

    [Fact]
    public void CustomDisplayNameShouldBeUsedInVersion()
    {
        CreateBeatmapFolder(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");
        BeatmapExtension songOne = CreateBeatmap(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");
        MergedOszBeatmap item = MergedOszBeatmap.FromBeatmap(songOne);
        item.DisplayName = "Custom Name";

        MergedOszExporter exporter = new(_songsDirectory, _saveDirectory);
        _ = exporter.Export([MergedOszBeatmap.CreatePlaceholder(), item], PackName, PackCreator, "");

        string entryList = GetEntryList(Path.Combine(_saveDirectory, $"{PackName}.osz"));
        Assert.Contains("Various Artists - Test Pack (Packer) [Custom Name].osu", entryList);
    }

    [Fact]
    public void MissingSourceFilesShouldFailThatItemButExportRest()
    {
        // No files created on disk, only the beatmap entry
        BeatmapExtension songOne = CreateBeatmap(1, "Song One", "Creator One", "audio1.mp3", "bg1.jpg", "song1");

        MergedOszExporter exporter = new(_songsDirectory, _saveDirectory);
        IReadOnlyList<MergedOszExporter.FailedExport> failed = exporter.Export(
            [MergedOszBeatmap.CreatePlaceholder(), MergedOszBeatmap.FromBeatmap(songOne)],
            PackName,
            PackCreator,
            "");

        Assert.Single(failed);
        Assert.True(File.Exists(Path.Combine(_saveDirectory, $"{PackName}.osz")));
    }

    private BeatmapExtension CreateBeatmap(int setId, string title, string creator, string audioName, string backgroundName, string tags)
        => new()
        {
            Dir = $"{setId} {title} ({creator})",
            OsuFileName = "map.osu",
            TitleRoman = title,
            ArtistRoman = "Artist",
            Creator = creator,
            DiffName = "Another",
            Mp3Name = audioName,
            Md5 = $"md5{setId}",
            MapSetId = setId * 100,
            MapId = setId * 200,
            Tags = tags,
            PlayMode = PlayMode.OsuMania,
        };

    private void CreateBeatmapFolder(int setId, string title, string creator, string audioName, string backgroundName, string tags)
    {
        string folder = Path.Combine(_songsDirectory, $"{setId} {title} ({creator})");
        Directory.CreateDirectory(folder);

        string osuContent = $"""
            osu file format v14

            [General]
            AudioFilename: {audioName}
            Mode: 3

            [Metadata]
            Title:{title}
            TitleUnicode:{title}
            Artist:Artist
            ArtistUnicode:Artist
            Creator:{creator}
            Version:Another
            Tags:{tags}
            BeatmapID:{setId * 200}
            BeatmapSetID:{setId * 100}

            [Difficulty]
            HPDrainRate:8
            CircleSize:4
            OverallDifficulty:8
            ApproachRate:5
            SliderMultiplier:1.4
            SliderTickRate:1

            [Events]
            //Background and Video events
            0,0,"{backgroundName}",0,0
            //Break Periods

            [HitObjects]
            192,192,15,5,0,0:0:0:0:
            64,192,99999999,5,0,0:0:0:0:
            """;

        File.WriteAllText(Path.Combine(folder, "map.osu"), osuContent);
        File.WriteAllBytes(Path.Combine(folder, audioName), [0x01, 0x02, 0x03]);
        File.WriteAllBytes(Path.Combine(folder, backgroundName), [0x01, 0x02, 0x03]);
    }

    private static string GetEntryList(string oszPath)
    {
        using IArchive archive = ZipArchive.Open(oszPath);
        return string.Join('\n', archive.Entries.Select(e => e.Key));
    }
}