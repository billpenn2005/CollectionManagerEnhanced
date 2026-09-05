namespace CollectionManager.Extensions.Modules.Downloader.Mirrors;

using CollectionManager.Extensions.Modules.Downloader.Api;
using System.ComponentModel;

/// <summary>
/// Download manager that uses the anonymous mirror candidates attached to each
/// <see cref="DownloadItem"/> (see <see cref="DownloadItem.Candidates"/>).
/// On a failure the base manager automatically retries with the next mirror
/// (<see cref="DownloadManager.TrySwitchMirror"/>); when all mirrors fail the
/// item is reported as errored. Requires no login — configured through a
/// download source with <c>RequiresLogin=false</c> in downloadSources.json.
/// </summary>
public class MirrorDownloader : DownloadManager
{
    public DownloadThrottler DownloadThrottler { get; private set; }

    public MirrorDownloader(string saveLocation, int downloadThreads, int downloadsPerMinute, int downloadsPerHour) : base(saveLocation, downloadThreads)
    {
        DownloadThrottler = new DownloadThrottler(downloadsPerMinute, downloadsPerHour);
    }

    public override bool CanDownload(DownloadItem downloadItem)
    {
        if (DownloadThrottler.CanDownload())
        {
            downloadItem.DownloadSlotStatus = null;
            return true;
        }

        downloadItem.DownloadSlotStatus = DownloadThrottler.GetStatus();
        return false;
    }

    protected override void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
    {
        if (e.Error == null && !e.Cancelled)
        {
            DownloadThrottler.RegisterDownload();
        }

        base.DownloadCompleted(sender, e);
    }
}