namespace CollectionManager.Extensions.Tests.Modules.API.OsuCollector;

using CollectionManager.Core.Modules.FileIo.OsuDb;
using CollectionManager.Core.Types;
using CollectionManager.Extensions.Modules.API.OsuCollector;
using CollectionManager.Extensions.Modules.Downloader.Api;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Xunit;

public class OsuCollectorApiTests
{
    private const string SampleJson = """
        {
          "id": 23199,
          "name": "Test Collection",
          "description": "A test description",
          "uploader": { "id": 123, "username": "testuser" },
          "beatmapsets": [
            { "id": 100, "beatmaps": [ { "id": 200, "checksum": "abc123" }, { "id": 201, "checksum": "def456" } ] },
            { "id": 300, "beatmaps": [ { "id": 400, "checksum": "ghi789" } ] }
          ],
          "favourites": 5
        }
        """;

    [Theory]
    [InlineData("23199", 23199)]
    [InlineData(" 23199 ", 23199)]
    [InlineData("https://osucollector.com/collections/23199", 23199)]
    [InlineData("https://osucollector.com/collections/23199/", 23199)]
    [InlineData("HTTPS://OSUCOLLECTOR.COM/collections/23199", 23199)]
    public void ParseCollectionIdShouldAcceptValidInputs(string input, int expected)
    {
        Assert.Equal(expected, OsuCollectorApi.ParseCollectionId(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1234567890123")]
    [InlineData("http://osucollector.com/collections/23199")]
    [InlineData("https://google.com/collections/23199")]
    [InlineData("https://osucollector.com/other/23199")]
    [InlineData("https://osucollector.com/collections/")]
    [InlineData("https://osucollector.com/collections/23199/extra")]
    [InlineData("https://osucollector.com")]
    [InlineData("osucollector.com/collections/23199")]
    public void ParseCollectionIdShouldRejectInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => OsuCollectorApi.ParseCollectionId(input));
    }

    [Fact]
    public void ShouldDeserializeApiResponse()
    {
        OsuCollectorCollection collection = JsonConvert.DeserializeObject<OsuCollectorCollection>(SampleJson);

        Assert.NotNull(collection);
        Assert.Equal(23199, collection.Id);
        Assert.Equal("Test Collection", collection.Name);
        Assert.Equal("A test description", collection.Description);
        Assert.Equal("testuser", collection.Uploader.Username);
        Assert.Equal(123, collection.Uploader.Id);
        Assert.Equal(2, collection.Beatmapsets.Count);
        Assert.Equal(3, collection.BeatmapCount);
        Assert.Equal(5, collection.Favourites);
        Assert.Equal("abc123", collection.Beatmapsets[0].Beatmaps[0].Checksum);
        Assert.Equal(200, collection.Beatmapsets[0].Beatmaps[0].Id);
        Assert.Equal(100, collection.Beatmapsets[0].Id);
    }

    [Fact]
    public void ToOsuCollectionShouldMapChecksumsAndIds()
    {
        OsuCollectorCollection collection = JsonConvert.DeserializeObject<OsuCollectorCollection>(SampleJson);
        OsuCollection osuCollection = new OsuCollectorApi().ToOsuCollection(collection, new MapCacher());

        Assert.Equal("Test Collection", osuCollection.Name);
        Assert.Equal(23199, osuCollection.OnlineId);
        Assert.Equal(3, osuCollection.NumberOfBeatmaps);
        Assert.Contains("abc123", osuCollection.BeatmapHashes);

        BeatmapExtension map = osuCollection.AllBeatmaps().First(m => m.Md5 == "def456");
        Assert.Equal(201, map.MapId);
        Assert.Equal(100, map.MapSetId);
        // Maps not present locally with a MapSetId land in the downloadable set
        Assert.Equal(3, osuCollection.DownloadableBeatmaps.Count);
    }

    [Fact]
    public void IsAlreadyImportedShouldMatchOnlineIds()
    {
        OsuCollection imported = new(new MapCacher()) { Name = "Imported", OnlineId = 23199 };
        OsuCollections collections = new() { imported };

        Assert.True(OsuCollectorApi.IsAlreadyImported(23199, collections));
        Assert.False(OsuCollectorApi.IsAlreadyImported(9999, collections));
    }
}

public class MirrorSwitchTests
{
    private class TestDownloadManager : DownloadManager
    {
        public TestDownloadManager(string saveLocation, int downloadThreads) : base(saveLocation, downloadThreads)
        {
        }

        public override bool CanDownload(DownloadItem downloadItem) => true;

        public bool SwitchMirror(DownloadItem item) => TrySwitchMirror(item);
    }

    private static DownloadItem CreateItemWithCandidates(int candidateCount)
    {
        DownloadItem item = new() { FileName = "test.osz", Url = "https://mirror0.example/test", Candidates = Enumerable.Range(1, candidateCount)
            .Select(i => new DownloadCandidate { Name = $"mirror{i}", Url = $"https://mirror{i}.example/test" }).ToList() };
        return item;
    }

    [Fact]
    public void ShouldNotSwitchWithoutCandidates()
    {
        using TestDownloadManager manager = new(Path.GetTempPath(), 1);
        DownloadItem item = new() { FileName = "test.osz", Url = "https://example.com/test" };

        Assert.False(manager.SwitchMirror(item));
        Assert.Equal(0, item.CurrentMirrorIndex);
    }

    [Fact]
    public void ShouldAdvanceToNextMirrorOnFailure()
    {
        using TestDownloadManager manager = new(Path.GetTempPath(), 1);
        DownloadItem item = CreateItemWithCandidates(9);
        item.Error = "timeout";

        Assert.True(manager.SwitchMirror(item));
        Assert.Equal(1, item.CurrentMirrorIndex);
        Assert.False(item.OtherError);
        Assert.Equal("", item.Error);
    }

    [Fact]
    public void ShouldNotSwitchPastLastMirror()
    {
        using TestDownloadManager manager = new(Path.GetTempPath(), 1);
        DownloadItem item = CreateItemWithCandidates(2);
        item.CurrentMirrorIndex = 1;

        Assert.False(manager.SwitchMirror(item));
        Assert.Equal(1, item.CurrentMirrorIndex);
    }
}