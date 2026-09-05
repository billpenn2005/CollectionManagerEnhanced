namespace CollectionManager.Core.Tests.Modules.FileIo;

using CollectionManager.Core.Modules.FileIo;
using CollectionManager.Core.Types;
using System;
using System.IO;
using Xunit;

public class BeatmapFileParserTests
{
    private static readonly string SampleOsu = """
        osu file format v14

        [General]
        AudioFilename: audio.mp3
        Mode: 3

        [Metadata]
        Title:Test Song
        TitleUnicode:テストソング
        Artist:Test Artist
        ArtistUnicode:テストアーティスト
        Creator:TestCreator
        Version:Another
        BeatmapID:123456
        BeatmapSetID:654321

        [Difficulty]
        HPDrainRate:6
        CircleSize:4
        OverallDifficulty:7
        ApproachRate:8
        """;

    [Fact]
    public void ShouldParseMetadataSections()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cm_parser_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string file = Path.Combine(dir, "Test Song - Test Song (TestCreator) [Another].osu");
        File.WriteAllText(file, SampleOsu);
        try
        {
            BeatmapExtension beatmap = BeatmapFileParser.Parse(file);

            Assert.Equal("Test Song", beatmap.Title);
            Assert.Equal("Test Artist", beatmap.Artist);
            Assert.Equal("TestCreator", beatmap.Creator);
            Assert.Equal("Another", beatmap.DiffName);
            Assert.Equal(123456, beatmap.MapId);
            Assert.Equal(654321, beatmap.MapSetId);
            Assert.Equal(Enums.PlayMode.OsuMania, beatmap.PlayMode);
            Assert.Equal(4f, beatmap.CircleSize);
            Assert.Equal(8f, beatmap.ApproachRate);
            Assert.Equal(7f, beatmap.OverallDifficulty);
            Assert.Equal("Test Song - Test Song (TestCreator) [Another].osu", beatmap.OsuFileName);
            Assert.Equal(Path.GetFileName(dir), beatmap.Dir);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ShouldComputeLegacyHashFromFileName()
    {
        string dir = Path.Combine(Path.GetTempPath(), "cm_parser_hash_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string file = Path.Combine(dir, "Some File Name.osu");
        File.WriteAllText(file, SampleOsu);
        try
        {
            BeatmapExtension beatmap = BeatmapFileParser.Parse(file);
            // MD5 of "SomeFileName.osu" (spaces removed) - verify length and repeatability
            Assert.Equal(32, beatmap.Md5.Length);
            Assert.Equal(beatmap.Md5, BeatmapFileParser.Parse(file).Md5);
            Assert.True(beatmap.Md5.All(c => Uri.IsHexDigit(c)));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}