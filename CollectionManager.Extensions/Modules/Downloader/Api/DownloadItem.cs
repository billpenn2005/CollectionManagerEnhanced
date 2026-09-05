namespace CollectionManager.Extensions.Modules.Downloader.Api;

using CollectionManager.Common.Interfaces;
using System;

public class DownloadItem : IDownloadItem
{
    public EventHandler DownloadUpdated;
    private void OnDownloadUpdated() => DownloadUpdated?.Invoke(this, EventArgs.Empty);
    public long Id { get; set; }
    public string Url { get; set; }
    public string FileName { get; set; }
    public string Name => FileName;
    public int RequestTimeout { get; set; }
    public string Progress
    {
        get
        {
            if (IsPaused)
            {
                return "Paused";
            }

            if (OtherError)
            {
                return Error;
            }

            if (DownloadAborted)
            {
                return "Download cancelled";
            }

            if (DownloadAborted)
            {
                return "Download cancelled";
            }

            if (BytesRecived > 0)
            {
                return string.Format("{0}/{1}MB {2}%", (BytesRecived / 1024f / 1024f).ToString("F"),
                (TotalBytes / 1024f / 1024f).ToString("F"), ProgressPrecentage);
            }

            if (!string.IsNullOrEmpty(DownloadSlotStatus))
            {
                return DownloadSlotStatus;
            }

            if (FileAlreadyExists)
            {
                return "File already exists";
            }

            if (WebClient != null)
            {
                return "Starting download...";
            }

            return "---";
        }
    }
    public long BytesRecived { get; set; }
    public long TotalBytes { get; set; }
    private int _progressPrecentage;
    public int ProgressPrecentage
    {
        get => _progressPrecentage;
        set
        {
            _progressPrecentage = value;
            OnDownloadUpdated();
        }
    }

    public bool DownloadAborted { get; set; }
    public bool FileAlreadyExists { get; set; }
    public string DownloadSlotStatus { get; set; }
    public bool OtherError { get; set; }
    public string Error { get; set; }
    public CookieAwareWebClient WebClient { get; set; }
    public int lastShownDlState { get; set; } = -1;
    public object UserToken { get; set; }
    public string Referer { get; set; }

    /// <summary>True while the item is individually paused (not downloaded by the queue).</summary>
    public bool IsPaused { get; set; }

    /// <summary>True when the item was removed by the user; it will never download again.</summary>
    public bool Removed { get; set; }

    /// <summary>Current download speed in bytes per second (0 when not downloading).</summary>
    public double DownloadSpeed { get; set; }

    /// <summary>Name of the mirror candidate currently used, when mirror candidates exist.</summary>
    public string CurrentMirrorName => Candidates is { Count: > 0 } candidates
        ? candidates[Math.Min(CurrentMirrorIndex, candidates.Count - 1)].Name
        : null;

    /// <summary>Human readable current download speed (empty when not downloading).</summary>
    public string SpeedText => DownloadSpeed > 0 ? $"{DownloadSpeed / 1024.0:F0} KB/s" : string.Empty;

    /// <summary>Short lifecycle status used by the download manager UI.</summary>
    public string Status
    {
        get
        {
            if (Removed) return "Removed";
            if (IsPaused) return "Paused";
            if (OtherError) return "Error";
            if (DownloadAborted) return "Cancelled";
            if (FileAlreadyExists) return "Already exists";
            if (WebClient?.IsBusy == true) return "Downloading";
            return "Queued";
        }
    }

    /// <summary>
    /// Alternative download URLs (anonymous mirrors) tried in order after failures.
    /// When set, <see cref="Url"/> is refreshed from the current candidate before each attempt.
    /// </summary>
    public IReadOnlyList<DownloadCandidate> Candidates { get; set; }

    /// <summary>Index into <see cref="Candidates"/> of the mirror currently being tried.</summary>
    public int CurrentMirrorIndex { get; set; }

    public void ResetErrorState()
    {
        Error = "";
        OtherError = false;
        DownloadAborted = false;
        FileAlreadyExists = false;
    }
    public override string ToString() => "DLitem: " + Url + " ; " + FileName;
}

/// <summary>One anonymous mirror URL candidate for a download, with its display name.</summary>
public class DownloadCandidate
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Referer { get; set; }
}
