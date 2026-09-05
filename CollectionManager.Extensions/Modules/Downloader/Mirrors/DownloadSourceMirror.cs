namespace CollectionManager.Extensions.Modules.Downloader.Mirrors;

/// <summary>
/// One anonymous beatmap mirror configured in <c>downloadSources.json</c>.
/// Both templates use <c>{0}</c> as the beatmapset ID placeholder.
/// </summary>
public class DownloadSourceMirror
{
    /// <summary>Display name shown in the download window (e.g. "osu.direct").</summary>
    public string Name { get; set; }

    /// <summary>Full (with video) download URL template, e.g. <c>https://osu.direct/d/{0}</c>.</summary>
    public string TemplateUrl { get; set; }

    /// <summary>No-video download URL template (may equal <see cref="TemplateUrl"/> when the mirror has no no-video variant).</summary>
    public string TemplateUrlNoVideo { get; set; }

    /// <summary>Optional Referer header value sent with the request.</summary>
    public string Referer { get; set; }
}