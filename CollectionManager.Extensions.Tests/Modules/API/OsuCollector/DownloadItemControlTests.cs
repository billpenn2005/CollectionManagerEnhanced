namespace CollectionManager.Extensions.Tests.Modules.API.OsuCollector;

using CollectionManager.Extensions.Modules.Downloader.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public class DownloadItemControlTests
{
    private class TestDownloadManager : DownloadManager
    {
        public TestDownloadManager(string saveLocation, int downloadThreads) : base(saveLocation, downloadThreads)
        {
        }

        public override bool CanDownload(DownloadItem downloadItem) => true;
    }

    private static DownloadItem CreateFailedItem()
    {
        DownloadItem item = new()
        {
            FileName = "test.osz",
            Url = "https://mirror0.example/test",
            Candidates = Enumerable.Range(1, 3).Select(i => new DownloadCandidate
            {
                Name = $"mirror{i}",
                Url = $"https://mirror{i}.example/test"
            }).ToList(),
            OtherError = true,
            Error = "Fatal error: timeout"
        };
        return item;
    }

    [Fact]
    public void PauseAndResumeShouldToggleFlag()
    {
        using TestDownloadManager manager = new(Path.GetTempPath(), 1);
        DownloadItem item = new() { FileName = "test.osz", Url = "https://example.com/test" };

        manager.PauseItem(item);
        Assert.True(item.IsPaused);
        Assert.Equal("Paused", item.Status);

        manager.ResumeItem(item);
        Assert.False(item.IsPaused);
    }

    [Fact]
    public void RemoveShouldMarkItemAsRemoved()
    {
        using TestDownloadManager manager = new(Path.GetTempPath(), 1);
        DownloadItem item = new() { FileName = "test.osz", Url = "https://example.com/test" };

        manager.RemoveItem(item);
        Assert.True(item.Removed);
        Assert.Equal("Removed", item.Status);
    }

    [Fact]
    public void RetryShouldResetErrorState()
    {
        using TestDownloadManager manager = new(Path.GetTempPath(), 1);
        DownloadItem item = CreateFailedItem();

        Assert.True(manager.RetryItem(item));
        Assert.False(item.OtherError);
        Assert.Equal("", item.Error);
        Assert.Equal(0, item.ProgressPrecentage);
    }

    [Fact]
    public void RetryShouldRejectActiveOrRemovedItems()
    {
        using TestDownloadManager manager = new(Path.GetTempPath(), 1);
        DownloadItem removed = CreateFailedItem();
        manager.RemoveItem(removed);

        Assert.False(manager.RetryItem(removed));

        DownloadItem active = new() { FileName = "x.osz", Url = "https://example.com", WebClient = new CookieAwareWebClient() };
        Assert.True(manager.RetryItem(active)); // not actively downloading -> requeued
        Assert.False(active.OtherError);
    }

    [Fact]
    public void ManualSwitchMirrorShouldWrapAround()
    {
        using TestDownloadManager manager = new(Path.GetTempPath(), 1);
        DownloadItem item = CreateFailedItem();
        item.CurrentMirrorIndex = 1;

        Assert.True(manager.SwitchMirror(item));
        Assert.Equal(2, item.CurrentMirrorIndex);
        Assert.False(item.OtherError);

        Assert.True(manager.SwitchMirror(item));
        Assert.Equal(0, item.CurrentMirrorIndex);
    }

    [Fact]
    public void ManualSwitchMirrorShouldFailWithoutCandidates()
    {
        using TestDownloadManager manager = new(Path.GetTempPath(), 1);
        DownloadItem item = new() { FileName = "test.osz", Url = "https://example.com/test" };

        Assert.False(manager.SwitchMirror(item));
        Assert.Equal(0, item.CurrentMirrorIndex);
    }
}