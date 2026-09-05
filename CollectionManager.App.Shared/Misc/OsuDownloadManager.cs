namespace CollectionManager.App.Shared.Misc;

using CollectionManager.App.Shared.Models;
using CollectionManager.Common;
using CollectionManager.Common.Interfaces;
using CollectionManager.Common.Interfaces.Forms;
using CollectionManager.Core.Types;
using CollectionManager.Extensions.Modules.Downloader.Api;
using CollectionManager.Extensions.Modules.Downloader.Mirrors;
using CollectionManager.Extensions.Utils;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

public sealed class OsuDownloadManager
{
    public static OsuDownloadManager Instance = new();

    /// <summary>
    /// Built-in default configuration used when no downloadSources.json and no saved
    /// settings exist. Kept in the program so mirror sources work out of the box.
    /// </summary>
    private const string DefaultSourcesJson = """
        [
          {
            "Name": "osu!",
            "Description": "official map download source.\r\nIn order to download maps you need to login using osu! cookies.\r\nInstructions: https://streamable.com/lhlr3d \r\nIf you still need help join discord and read pinned message in #cm-help.\r\nhttps://discord.gg/N854wYZ",
            "Referer": "https://osu.ppy.sh/beatmapsets/",
            "BaseDownloadUrl": "https://osu.ppy.sh/beatmapsets/{0}/download",
            "ThrottleDownloads": true,
            "DownloadsPerMinute": 3,
            "DownloadsPerHour": 170,
            "DownloadThreads": 3,
            "RequestTimeout": 5000,
            "FullyQualifiedHandlerName": "CollectionManager.Extensions.Modules.Downloader.OsuDownloader, CollectionManager.Extensions",
            "RequiresLogin": true,
            "UseCookiesLogin": true
          },
          {
            "Name": "osu mirrors (anonymous)",
            "Description": "Community mirrors, no login required. On failure downloads automatically retry with the next mirror.",
            "Referer": "",
            "BaseDownloadUrl": "https://mirror.hinamizawa.ai/api/v1/hinai/d/{0}",
            "ThrottleDownloads": true,
            "DownloadsPerMinute": 3,
            "DownloadsPerHour": 170,
            "DownloadThreads": 3,
            "RequestTimeout": 10000,
            "FullyQualifiedHandlerName": "CollectionManager.Extensions.Modules.Downloader.Mirrors.MirrorDownloader, CollectionManager.Extensions",
            "RequiresLogin": false,
            "UseCookiesLogin": false,
            "Mirrors": [
              { "Name": "osu.direct", "TemplateUrl": "https://osu.direct/d/{0}", "TemplateUrlNoVideo": "https://osu.direct/d/{0}n", "Referer": "" },
              { "Name": "Nerinyan", "TemplateUrl": "https://api.nerinyan.moe/d/{0}", "TemplateUrlNoVideo": "https://api.nerinyan.moe/d/{0}?nv=1", "Referer": "" },
              { "Name": "Sayobot", "TemplateUrl": "https://dl.sayobot.cn/beatmaps/download/full/{0}", "TemplateUrlNoVideo": "https://dl.sayobot.cn/beatmaps/download/novideo/{0}", "Referer": "" },
              { "Name": "Nekoha", "TemplateUrl": "https://mirror.nekoha.moe/api/download/{0}", "TemplateUrlNoVideo": "https://mirror.nekoha.moe/api/download/{0}", "Referer": "" },
              { "Name": "Beatconnect", "TemplateUrl": "https://beatconnect.io/b/{0}/", "TemplateUrlNoVideo": "https://beatconnect.io/b/{0}/?novideo=1", "Referer": "" },
              { "Name": "osu!dl", "TemplateUrl": "https://osudl.org/s/{0}", "TemplateUrlNoVideo": "https://osudl.org/s/{0}?video=false", "Referer": "" },
              { "Name": "catboy.best", "TemplateUrl": "https://catboy.best/d/{0}", "TemplateUrlNoVideo": "https://catboy.best/d/{0}n", "Referer": "" },
              { "Name": "Hinamizawa", "TemplateUrl": "https://mirror.hinamizawa.ai/api/v1/hinai/d/{0}", "TemplateUrlNoVideo": "https://mirror.hinamizawa.ai/api/v1/hinai/d/{0}?no_video=true", "Referer": "" },
              { "Name": "nzbasic", "TemplateUrl": "https://direct.nzbasic.com/{0}.osz", "TemplateUrlNoVideo": "https://direct.nzbasic.com/{0}.osz", "Referer": "" }
            ]
          }
        ]
        """;

    //Collections for preparing downloads
    private Beatmaps BeatmapsToDownload { get; } = [];
    private HashSet<int> ListedMapSetIds { get; } = [];
    /// <summary>
    /// Stores all requested downloads
    /// </summary>
    public ICollection<IDownloadItem> DownloadItems { get; private set; } = [];

    private DownloadManager _mapDownloader;
    private List<DownloadSource> _downloadSources;
    private long _lastProgressUpdateTick;

    private List<DownloadSource> LoadDownloadSources()
    {
        string configLocation = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "downloadSources.json");

        // 1. configuration saved through the in-app "Download sources" settings window
        if (!string.IsNullOrWhiteSpace(Initalizer.Settings?.DownloadSourcesJson))
        {
            try
            {
                List<DownloadSource> saved = JsonConvert.DeserializeObject<List<DownloadSource>>(Initalizer.Settings.DownloadSourcesJson);
                if (saved is { Count: > 0 })
                {
                    return saved;
                }
            }
            catch
            {
                // fall through to the file / built-in defaults
            }
        }

        // 2. optionally hand-edited downloadSources.json next to the executable
        if (File.Exists(configLocation))
        {
            try
            {
                List<DownloadSource> sources = JsonConvert.DeserializeObject<List<DownloadSource>>(File.ReadAllText(configLocation));
                if (sources is { Count: > 0 })
                {
                    return sources;
                }
            }
            catch
            {
                // fall through to the built-in defaults
            }
        }

        // 3. built-in defaults (official source + anonymous mirrors)
        return JsonConvert.DeserializeObject<List<DownloadSource>>(DefaultSourcesJson);
    }

    private IReadOnlyList<IDownloadSource> DownloadSources => _downloadSources ??= LoadDownloadSources();

    /// <summary>Returns the current download source list (for the settings UI).</summary>
    public IReadOnlyList<IDownloadSource> GetDownloadSources() => DownloadSources;

    /// <summary>Saves the download source list (falling back to the file next to the exe).</summary>
    public void SaveDownloadSources(List<DownloadSource> sources)
    {
        _downloadSources = sources;
        if (Initalizer.Settings is not null)
        {
            Initalizer.Settings.DownloadSourcesJson = JsonConvert.SerializeObject(sources);
            Initalizer.Settings.Save();
        }

        try
        {
            string configLocation = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "downloadSources.json");
            File.WriteAllText(configLocation, JsonConvert.SerializeObject(sources, Formatting.Indented));
        }
        catch
        {
            // executable directory may be read-only; the settings entry still persists
        }
    }
    public IDownloadSource SelectedDownloadSource { get; private set; }
    public event EventHandler DownloadItemsChanged;
    public event EventHandler<DownloadItem> DownloadItemUpdated;

    /// <summary>Raised when a download completes and its final .osz file is in place.</summary>
    public event EventHandler DownloadFinished;
    private readonly HashSet<long> _downloadFinishedNotified = [];
    /// <summary>Re-checks completed downloads until their final file is in place (moveTemp runs on the downloader's 5s watcher).</summary>
    private readonly System.Threading.Timer _downloadFinishedWatcher;

    public OsuDownloadManager()
    {
        _downloadFinishedWatcher = new System.Threading.Timer(_ => NotifyDownloadFinishedIfNeeded());
    }

    public bool IsLoggedIn { get; private set; }
    public string DownloadDirectory { get; set; } = string.Empty;
    public bool DownloadDirectoryIsSet => !string.IsNullOrEmpty(DownloadDirectory);
    private long _downloadId;
    public bool? DownloadWithVideo { get; set; }

    public async Task<bool> AskUserForSaveDirectoryAndLoginAsync(IUserDialogs userDialogs, ILoginFormView loginForm)
    {
        const string loginFailedMessage = "Login failed. Ensure that your login/password or cookies are correct";

        if (IsLoggedIn)
        {
            return true;
        }

        DownloaderSettings downloaderSettings = JsonConvert.DeserializeObject<DownloaderSettings>(Initalizer.Settings.DownloadManager_DownloaderSettings);
        downloaderSettings.LoginData ??= new LoginData();

        bool useExistingSettings = downloaderSettings.IsValid(DownloadSources)
            && await userDialogs.YesNoMessageBoxAsync($"Reuse last downloader settings? {Environment.NewLine}{downloaderSettings}", "DownloadManager - Reuse settings", MessageBoxType.Question);

        if (useExistingSettings)
        {
            DownloadDirectory = downloaderSettings.DownloadDirectory;
            DownloadWithVideo = downloaderSettings.DownloadWithVideo;

            if (TryLogIn(downloaderSettings.LoginData))
            {
                return true;
            }

            await userDialogs.OkMessageBoxAsync(loginFailedMessage, "Error", MessageBoxType.Error);

            return false;
        }

        DownloadDirectory = await userDialogs.SelectDirectoryAsync("Select directory for saved beatmaps", true);
        if (!DownloadDirectoryIsSet)
        {
            return false;
        }

        DownloadWithVideo = await userDialogs.YesNoMessageBoxAsync("Download beatmaps with video?", "Beatmap downloader", MessageBoxType.Question);
        LoginData userLoginData = loginForm.GetLoginData(DownloadSources);
        if (TryLogIn(userLoginData))
        {
            Initalizer.Settings.DownloadManager_DownloaderSettings = JsonConvert.SerializeObject(new DownloaderSettings
            {
                DownloadWithVideo = DownloadWithVideo,
                DownloadDirectory = DownloadDirectory,
                LoginData = userLoginData
            });
        }
        else
        {
            await userDialogs.OkMessageBoxAsync(loginFailedMessage, "Error", MessageBoxType.Error);
        }

        return IsLoggedIn;
    }

    public void DownloadBeatmap(Beatmap beatmap) => DownloadBeatmap(beatmap, true);

    public void PauseDownloads() => _mapDownloader?.StopDownloads = true;
    public void ResumeDownloads() => _mapDownloader?.StopDownloads = false;

    /// <summary>
    /// Switches the active download source at runtime. Queued items are migrated to the new
    /// downloader (URLs and mirror candidates rebuilt for the new source); a download that is
    /// currently in flight finishes on the old downloader untouched. Returns false when the
    /// name is unknown or already selected.
    /// </summary>
    public bool ChangeSelectedDownloadSource(string name)
    {
        DownloadSource source = DownloadSources.OfType<DownloadSource>().FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        if (source == null || source.Name == SelectedDownloadSource?.Name)
        {
            return false;
        }

        List<DownloadItem> pending = [];
        List<DownloadItem> userPaused = [];
        if (_mapDownloader != null)
        {
            // Park the queue. The old downloader is NOT disposed: an in-flight download keeps
            // running there and its completed file is still moved into place by its watcher.
            _mapDownloader.StopDownloads = true;
            pending.AddRange(_mapDownloader.GetPendingItems());
            userPaused.AddRange(pending.Where(i => i.IsPaused));
            foreach (DownloadItem item in pending)
            {
                item.IsPaused = true; // keep items parked while the new downloader is built
            }
        }

        SelectedDownloadSource = source;
        IsLoggedIn = !SelectedDownloadSource.RequiresLogin;
        _mapDownloader = CreateDownloader();

        foreach (DownloadItem item in pending)
        {
            item.IsPaused = false;
            item.ResetTransferState();
            RebuildItemForSource(item);
            _mapDownloader.EnqueueItem(item);
            item.IsPaused = userPaused.Contains(item); // keep user-paused items paused on the new source
        }

        DownloadItemsChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>Rebuilds the URL and mirror candidates of an item for <see cref="SelectedDownloadSource"/>.</summary>
    private void RebuildItemForSource(DownloadItem item)
    {
        item.Url = BuildDownloadUrl(item.MapSetId);
        List<DownloadSourceMirror> mirrors = (SelectedDownloadSource as DownloadSource)?.Mirrors;
        if (mirrors is { Count: > 0 })
        {
            item.Candidates = [.. mirrors.Select(mirror => new DownloadCandidate
            {
                Name = mirror.Name,
                Url = string.Format(DownloadWithVideo == true ? mirror.TemplateUrl : mirror.TemplateUrlNoVideo, item.MapSetId),
                Referer = string.IsNullOrEmpty(mirror.Referer) ? string.Format(SelectedDownloadSource.Referer, item.MapSetId) : mirror.Referer
            })];
            item.CurrentMirrorIndex = 0;
        }
        else
        {
            item.Candidates = null;
            item.CurrentMirrorIndex = 0;
        }
    }

    private string BuildDownloadUrl(int mapSetId)
    {
        string noVideoSuffix = DownloadWithVideo != null && DownloadWithVideo.Value ? string.Empty : "?noVideo=1";
        return string.Format(SelectedDownloadSource.BaseDownloadUrl, mapSetId) + noVideoSuffix;
    }

    private DownloadManager CreateDownloader()
    {
        Type downloaderType = Type.GetType(SelectedDownloadSource.FullyQualifiedHandlerName);
        if (downloaderType == null)
        {
            throw new NotImplementedException($"Download manager of type \"{SelectedDownloadSource.FullyQualifiedHandlerName}\" could not be found.");
        }

        DownloadManager downloader = (DownloadManager)Activator.CreateInstance(downloaderType, DownloadDirectory, SelectedDownloadSource.DownloadThreads, SelectedDownloadSource.DownloadsPerMinute, SelectedDownloadSource.DownloadsPerHour);
        downloader.ProgressUpdated += MapDownloaderOnProgressUpdated;
        return downloader;
    }

    public void PauseItems(IEnumerable<DownloadItem> items)
    {
        foreach (DownloadItem item in items)
        {
            _mapDownloader?.PauseItem(item);
        }

        DownloadItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResumeItems(IEnumerable<DownloadItem> items)
    {
        foreach (DownloadItem item in items)
        {
            _mapDownloader?.ResumeItem(item);
        }

        DownloadItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItems(IEnumerable<DownloadItem> items)
    {
        foreach (DownloadItem item in items)
        {
            _mapDownloader?.RemoveItem(item);
        }

        DownloadItems = [.. DownloadItems.Where(i => i is not DownloadItem item || !item.Removed)];
        DownloadItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RetryItems(IEnumerable<DownloadItem> items)
    {
        foreach (DownloadItem item in items)
        {
            _mapDownloader?.RetryItem(item);
        }

        DownloadItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SwitchMirrorItems(IEnumerable<DownloadItem> items)
    {
        foreach (DownloadItem item in items)
        {
            _mapDownloader?.SwitchMirror(item);
        }

        DownloadItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Switches items to a specific mirror by name (works while queued, paused, errored or downloading).</summary>
    public void SwitchMirrorItems(IEnumerable<DownloadItem> items, string mirrorName)
    {
        foreach (DownloadItem item in items)
        {
            _mapDownloader?.SwitchMirror(item, mirrorName);
        }

        DownloadItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void MapDownloaderOnProgressUpdated(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
    {
        long now = Environment.TickCount64;
        if (now - _lastProgressUpdateTick < 500)
        {
            return;
        }

        _lastProgressUpdateTick = now;
        DownloadItem item = (DownloadItem)downloadProgressChangedEventArgs.UserState;
        DownloadItemUpdated?.Invoke(this, item);
        NotifyDownloadFinishedIfNeeded();
    }

    /// <summary>Raises <see cref="DownloadFinished"/> for completed items whose final file is in place.</summary>
    private void NotifyDownloadFinishedIfNeeded()
    {
        foreach (DownloadItem item in DownloadItems.OfType<DownloadItem>())
        {
            if (item.Removed || item.IsPaused || _downloadFinishedNotified.Contains(item.Id) || !item.IsCompleted)
            {
                continue;
            }

            if (string.IsNullOrEmpty(DownloadDirectory) || !File.Exists(Path.Combine(DownloadDirectory, item.FileName)))
            {
                continue; // final .osz not moved into place yet
            }

            _downloadFinishedNotified.Add(item.Id);
            DownloadFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    internal void DownloadBeatmaps(Beatmaps selectedBeatmaps)
    {
        foreach (Beatmap selectedBeatmap in selectedBeatmaps)
        {
            DownloadBeatmap(selectedBeatmap, false);
        }

        DownloadBeatmap(null, true);
    }

    private bool TryLogIn(LoginData loginData)
    {
        if (string.IsNullOrEmpty(loginData.DownloadSource))
        {
            return false;
        }

        SelectedDownloadSource = DownloadSources.First(s => s.Name == loginData.DownloadSource);
        _mapDownloader = CreateDownloader();
        return SelectedDownloadSource.RequiresLogin ? (IsLoggedIn = loginData.IsValid() && _mapDownloader.Login(loginData)) : (IsLoggedIn = true);
    }

    private void DownloadBeatmap(Beatmap beatmap, bool fireUpdateEvent)
    {
        if (beatmap != null)
        {
            BeatmapsToDownload.Add((BeatmapExtension)beatmap);
            DownloadItem downloadItem = GetDownloadItem((BeatmapExtension)beatmap);
            if (downloadItem == null)
            {
                return;
            }

            DownloadItems.Add(downloadItem);
            _ = ListedMapSetIds.Add(beatmap.MapSetId);
        }

        if (fireUpdateEvent)
        {
            // start watching for completed downloads (final file placement + refresh notification)
            _downloadFinishedWatcher.Change(2000, 5000);
            DownloadItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private DownloadItem GetDownloadItem(Beatmap beatmap)
    {
        if (beatmap.MapSetId < 1 || ListedMapSetIds.Contains(beatmap.MapSetId))
        {
            return null;
        }

        long currentId = ++_downloadId;
        string oszFileName = beatmap.OszFileName();

        DownloadItem downloadItem = _mapDownloader.DownloadFile(BuildDownloadUrl(beatmap.MapSetId), oszFileName, string.Format(SelectedDownloadSource.Referer, beatmap.MapSetId), currentId, SelectedDownloadSource.RequestTimeout);
        downloadItem.MapSetId = beatmap.MapSetId;
        RebuildItemForSource(downloadItem);
        downloadItem.Id = currentId;
        return downloadItem;
    }
}