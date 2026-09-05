namespace CollectionManager.App.Shared.Models.Controls;

using CollectionManager.App.Shared.Interfaces.Controls;
using CollectionManager.App.Shared.Misc;
using CollectionManager.Common.Interfaces;
using CollectionManager.Extensions.Modules.Downloader.Api;
using System.Linq;

public class DownloadManagerModel : IDownloadManagerModel
{
    private readonly OsuDownloadManager _osuDownloadManager;
    public event EventHandler DownloadItemsChanged;
    public event EventHandler<DownloadItem> DownloadItemUpdated;
    public event EventHandler LogInStatusChanged;
    public event EventHandler LogInRequest;
    public event EventHandler StartDownloads;
    public event EventHandler StopDownloads;
    public void EmitStartDownloads()
    {
        StartDownloads?.Invoke(this, EventArgs.Empty);
        _osuDownloadManager?.ResumeDownloads();
    }

    public void EmitStopDownloads()
    {
        StopDownloads?.Invoke(this, EventArgs.Empty);
        _osuDownloadManager?.PauseDownloads();
    }

    public void EmitLoginRequest() => LogInRequest?.Invoke(this, EventArgs.Empty);

    public void PauseItems(IEnumerable<DownloadItem> items) => _osuDownloadManager?.PauseItems(items);

    public void ResumeItems(IEnumerable<DownloadItem> items) => _osuDownloadManager?.ResumeItems(items);

    public void RemoveItems(IEnumerable<DownloadItem> items) => _osuDownloadManager?.RemoveItems(items);

    public void RetryItems(IEnumerable<DownloadItem> items) => _osuDownloadManager?.RetryItems(items);

    public void SwitchMirrorItems(IEnumerable<DownloadItem> items) => _osuDownloadManager?.SwitchMirrorItems(items);

    public void SetDownloadSource(string name) => _osuDownloadManager?.ChangeSelectedDownloadSource(name);

    public IReadOnlyList<string> DownloadSourceNames => _osuDownloadManager.GetDownloadSources().Select(s => s.Name).ToList();

    public string SelectedDownloadSourceName => _osuDownloadManager.SelectedDownloadSource?.Name;

    private ICollection<IDownloadItem> _downloadItems;

    public ICollection<IDownloadItem> DownloadItems
    {
        get => _downloadItems;
        set
        {
            _downloadItems = value;
            DownloadItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool _isLoggedIn;

    public DownloadManagerModel(OsuDownloadManager osuDownloadManager)
    {
        _osuDownloadManager = osuDownloadManager;
        _downloadItems = _osuDownloadManager.DownloadItems;
        _osuDownloadManager.DownloadItemsChanged += OsuDownloadManagerOnDownloadItemsChanged;
        _osuDownloadManager.DownloadItemUpdated += OsuDownloadManagerOnDownloadItemUpdated;
    }

    private void OsuDownloadManagerOnDownloadItemUpdated(object sender, DownloadItem eventArgs) => DownloadItemUpdated?.Invoke(this, eventArgs);

    private void OsuDownloadManagerOnDownloadItemsChanged(object sender, EventArgs eventArgs) => DownloadItems = _osuDownloadManager.DownloadItems;

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            _isLoggedIn = value;
            LogInStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}