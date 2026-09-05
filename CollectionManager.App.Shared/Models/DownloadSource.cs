namespace CollectionManager.App.Shared.Models;

using CollectionManager.Common.Interfaces;
using CollectionManager.Extensions.Modules.Downloader.Mirrors;
using System.Collections.Generic;

public class DownloadSource : IDownloadSource
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Referer { get; set; }
    public string BaseDownloadUrl { get; set; }
    /// <summary>
    /// Optional anonymous mirror list. When present, every download carries all
    /// mirror URLs as candidates and failures automatically fall over to the
    /// next mirror (requires a <c>MirrorDownloader</c> handler).
    /// </summary>
    public List<DownloadSourceMirror> Mirrors { get; set; }
    public bool ThrottleDownloads { get; set; }
    public int DownloadsPerMinute { get; set; }
    public int DownloadsPerHour { get; set; }
    public int DownloadThreads { get; set; }
    public string FullyQualifiedHandlerName { get; set; }
    public bool RequiresLogin { get; set; }
    public bool UseCookiesLogin { get; set; }
    public int RequestTimeout { get; set; }
}