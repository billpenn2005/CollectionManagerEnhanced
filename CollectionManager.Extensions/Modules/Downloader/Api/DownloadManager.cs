namespace CollectionManager.Extensions.Modules.Downloader.Api;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public abstract class DownloadManager : IDisposable
{
    protected Queue<CookieAwareWebClient> Clients = new();
    private readonly LinkedList<DownloadItem> _urlsToDownload = new();
    private readonly ConcurrentQueue<FileWorkerArgs> FileOperations = new();
    private readonly Timer _urlWatcher;
    private readonly Timer _ProgressWatcher;
    private readonly string _saveLocation;

    public bool StopDownloads
    {
        get => _stopDownloads;
        set
        {
            lock (_lockingObject)
            {
                _stopDownloads = value;
            }
        }
    }

    private readonly Dictionary<int, DownloadProgress> downloadCheck = [];
    private readonly Dictionary<DownloadItem, HttpWebRequest> _activeRequests = [];
    private bool _stopDownloads;
    public event EventHandler<DownloadProgressChangedEventArgs> ProgressUpdated;
    private static readonly object _lockingObject = new();
    public DownloadManager(string saveLocation, int downloadThreads)
    {
        _saveLocation = saveLocation;

        for (int i = 0; i < downloadThreads; i++)
        {
            CookieAwareWebClient webClient = new()
            {
                ClientId = i
            };
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
            Clients.Enqueue(webClient);
            downloadCheck.Add(i, new DownloadProgress());
        }

        //Run callback every 500ms with null as state
        _urlWatcher = new Timer(Callback, null, 0, 250);
        _ProgressWatcher = new Timer(ProgressWatcher, null, 0, 5000);

    }

    public virtual bool Login(LoginData loginData) => true;

    public static void ChangeDefaultConnectionPolicy(int maxConnectionsToSameServer) => ServicePointManager.DefaultConnectionLimit = maxConnectionsToSameServer;

    private void ProgressWatcher(object state)
    {
        lock (_lockingObject)
        {
            foreach (KeyValuePair<int, DownloadProgress> dlItemCheck in downloadCheck)
            {
                if (dlItemCheck.Value.IsStalled())
                {
                    dlItemCheck.Value.downloadItem.WebClient.CancelAsync();
                    AbortActiveRequest(dlItemCheck.Value.downloadItem);
                }

                dlItemCheck.Value.Process();
            }
        }

        while (!FileOperations.IsEmpty)
        {
            if (FileOperations.TryDequeue(out FileWorkerArgs args))
            {
                switch (args.action)
                {
                    case "removeTemp":
                        try
                        {
                            if (File.Exists(args.orginalLocation))
                            {
                                File.Delete(args.orginalLocation);
                            }
                        }
                        catch (IOException) { }

                        break;
                    case "moveTemp":
                        if (File.Exists(args.orginalLocation))
                        {
                            if (!File.Exists(args.desiredLocation))
                            {
                                File.Move(args.orginalLocation, args.desiredLocation);
                            }
                        }

                        break;
                }
            }
            else
            {
                break;
            }
        }
    }
    private void Callback(object state)
    {
        //Main async download loop
        lock (_lockingObject)
        {
            if (StopDownloads)
            {
                foreach (KeyValuePair<int, DownloadProgress> dlItemCheck in downloadCheck)
                {
                    dlItemCheck.Value.downloadItem?.WebClient.CancelAsync();
                }
            }
            else
            {
                lock (_urlsToDownload)
                {
                    if (_urlsToDownload.Count > 0)
                    {
                        if (Clients.Count > 0)
                        {
                            DownloadItem downloadItem = _urlsToDownload.First.Value;
                            if (downloadItem.Removed)
                            {
                                _urlsToDownload.RemoveFirst();
                                return;
                            }

                            if (downloadItem.IsPaused)
                            {
                                // keep paused items out of the way of other downloads
                                _urlsToDownload.RemoveFirst();
                                _urlsToDownload.AddLast(downloadItem);
                                return;
                            }

                            if (!CanDownload(downloadItem))
                            {
                                return;
                            }

                            downloadItem.DownloadSlotStatus = "Starting download...";
                            CookieAwareWebClient client = Clients.Dequeue();
                            downloadItem.DownloadSlotStatus = null;
                            _urlsToDownload.RemoveFirst();
                            client.RequestTimeout = downloadItem.RequestTimeout;
                            downloadItem.WebClient = client;
                            _ = DownloadFile(downloadItem);
                        }
                    }
                }
            }
        }
    }

    public abstract bool CanDownload(DownloadItem downloadItem);

    protected virtual bool DownloadFile(DownloadItem downloadItem)
    {
        lock (_lockingObject)
        {
            if (downloadItem.IsDownloading)
            {
                return false; // already in flight (double enqueue guard)
            }

            string filePath = Path.Combine(_saveLocation, downloadItem.FileName);
            if (File.Exists(filePath))
            {
                downloadItem.FileAlreadyExists = true;
                downloadItem.ResumeOffset = 0;
                Clients.Enqueue(downloadItem.WebClient);
                return false;
            }

            if (downloadItem.Candidates is { Count: > 0 } candidates)
            {
                int index = Math.Min(downloadItem.CurrentMirrorIndex, candidates.Count - 1);
                downloadItem.Url = candidates[index].Url;
                downloadItem.DownloadSlotStatus = $"Downloading from {candidates[index].Name}...";
            }
            else
            {
                downloadItem.DownloadSlotStatus = "Downloading...";
            }

            downloadCheck[downloadItem.WebClient.ClientId].Reset();
            downloadItem.ResetErrorState();
            downloadCheck[downloadItem.WebClient.ClientId].downloadItem = downloadItem;
            downloadItem.WebClient.Headers["Referer"] = downloadItem.Referer;

            string tempFileLocation = GetFullTempLocation(downloadItem.FileName);
            long offset = 0;
            if (downloadItem.ResumeOffset > 0 && File.Exists(tempFileLocation) && downloadItem.ResumeOffset <= new FileInfo(tempFileLocation).Length)
            {
                offset = downloadItem.ResumeOffset;
            }

            if (offset > 0)
            {
                downloadItem.WebClient.Headers[HttpRequestHeader.Range] = $"bytes={offset}-";
                downloadItem.DownloadSlotStatus = $"Resuming from {(offset / 1024f / 1024f):F1}MB...";
            }
            else
            {
                downloadItem.WebClient.Headers.Remove(HttpRequestHeader.Range);
                downloadItem.ResumeOffset = 0;
            }

            downloadItem.DownloadSlotStatus ??= "Downloading...";
            downloadItem.IsDownloading = true;
            downloadItem.AbortRequested = false; // fresh start (previous pause / stop must not cancel this attempt)
            _ = DownloadFileAsync(downloadItem, tempFileLocation, offset);
            return true;
        }
    }

    /// <summary>
    /// Downloads the item body to the temp file ourselves (instead of WebClient.DownloadFileAsync)
    /// so a paused/stopped download can resume from the byte offset via a Range request, and so a
    /// partial temp file survives pausing (no more delete-on-pause).
    /// </summary>
    private async Task DownloadFileAsync(DownloadItem downloadItem, string tempFileLocation, long offset)
    {
        CookieAwareWebClient client = downloadItem.WebClient;
        bool cancelled = false;
        HttpWebRequest request = null;
        try
        {
            request = (HttpWebRequest)WebRequest.Create(downloadItem.Url);
            request.UserAgent = client.UserAgent;
            request.CookieContainer = client.CookieContainer;
            request.Timeout = client.RequestTimeout;
            request.AllowAutoRedirect = true;
            if (!string.IsNullOrEmpty(downloadItem.Referer))
            {
                request.Referer = downloadItem.Referer;
            }

            if (offset > 0)
            {
                request.AddRange(offset);
            }

            lock (_activeRequests)
            {
                _activeRequests[downloadItem] = request;
            }

            long total;
            WebResponse response = await request.GetResponseAsync();
            if (response == null)
            {
                // aborted via request.Abort() (pause / stop / remove / stall)
                DownloadCompleted(client, new AsyncCompletedEventArgs(null, true, downloadItem));
                return;
            }

            using (response)
            {
                total = offset + response.ContentLength;
                using Stream webStream = response.GetResponseStream();
                using FileStream fileStream = new(tempFileLocation, offset > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write);
                byte[] buffer = new byte[128 * 1024];
                long received = offset;
                while (true)
                {
                    // HttpWebRequest.Abort() does not interrupt an already-started response stream,
                    // so pausing/stopping is handled here: stop the copy loop and report as cancelled.
                    if (downloadItem.AbortRequested || downloadItem.Removed || downloadItem.IsPaused || StopDownloads)
                    {
                        DownloadCompleted(client, new AsyncCompletedEventArgs(null, true, downloadItem));
                        return;
                    }

                    int read = await webStream.ReadAsync(buffer);
                    if (read <= 0)
                    {
                        break;
                    }

                    await fileStream.WriteAsync(buffer.AsMemory(0, read));
                    received += read;
                    ReportTransferProgress(downloadItem, received, total);
                }
            }

            downloadItem.ResumeOffset = 0;
            downloadItem.DownloadSlotStatus = null;
            downloadItem.AbortRequested = false;
            DownloadCompleted(client, new AsyncCompletedEventArgs(null, false, downloadItem));
        }
        catch (Exception ex)
        {
            bool wasCancelled = downloadItem.AbortRequested || downloadItem.Removed || downloadItem.IsPaused || StopDownloads
                || ex is WebException { Status: WebExceptionStatus.RequestCanceled } || ex is ObjectDisposedException
                || (ex as IOException)?.Message?.Contains("aborted", StringComparison.OrdinalIgnoreCase) == true;
            DownloadCompleted(client, new AsyncCompletedEventArgs(wasCancelled ? null : ex, wasCancelled, downloadItem));
        }
        finally
        {
            lock (_activeRequests)
            {
                _activeRequests.Remove(downloadItem);
            }

            downloadItem.IsDownloading = false;
        }
    }

    private void ReportTransferProgress(DownloadItem downloadItem, long received, long total)
    {
        int percentage = total > 0 ? (int)(received * 100 / total) : 0;
        if (downloadItem.lastShownDlState != percentage)
        {
            downloadItem.lastShownDlState = percentage;
            DownloadProgressChangedEventArgs args = (DownloadProgressChangedEventArgs)Activator.CreateInstance(typeof(DownloadProgressChangedEventArgs),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [percentage, downloadItem, received, total], null);
            OnProgressUpdated(args);
        }

        DownloadProgress check = downloadCheck[downloadItem.WebClient.ClientId];
        long now = Environment.TickCount64;
        if (check.LastTick != 0)
        {
            long elapsedMs = now - check.LastTick;
            if (elapsedMs > 0)
            {
                downloadItem.DownloadSpeed = (received - check.bytesRecived) * 1000.0 / elapsedMs;
            }
        }

        check.LastTick = now;
        check.bytesRecived = received;
    }

    /// <summary>
    /// Called on a download failure before the item is marked as errored.
    /// Moves the item to the next mirror candidate (if any) and requeues it.
    /// Returns true when a retry on an alternative mirror was queued.
    /// </summary>
    protected virtual bool TrySwitchMirror(DownloadItem downloadItem)
    {
        if (downloadItem.Candidates is not { Count: > 0 } candidates || downloadItem.CurrentMirrorIndex + 1 >= candidates.Count)
        {
            return false;
        }

        downloadItem.CurrentMirrorIndex++;
        downloadItem.ResetTransferState(); // different mirror -> start over
        downloadItem.DownloadSlotStatus = $"Switching to {candidates[downloadItem.CurrentMirrorIndex].Name}...";
        lock (_urlsToDownload)
        {
            _urlsToDownload.AddFirst(downloadItem);
        }
        lock (_lockingObject)
        {
            Clients.Enqueue(downloadItem.WebClient);
        }
        return true;
    }

    /// <summary>Aborts the in-flight HTTP request of an item (used by pause / stop / remove / stall).</summary>
    private void AbortActiveRequest(DownloadItem downloadItem)
    {
        lock (_activeRequests)
        {
            if (_activeRequests.TryGetValue(downloadItem, out HttpWebRequest request))
            {
                downloadItem.AbortRequested = true;
                try
                {
                    request.Abort();
                }
                catch (ObjectDisposedException) { }
            }
        }
    }
    /// <summary>Pauses a single item. A currently running download is cancelled; the item stays queued (skipped) until resumed.</summary>
    public void PauseItem(DownloadItem downloadItem)
    {
        downloadItem.IsPaused = true;
        downloadItem.DownloadSlotStatus = null;
        if (downloadItem.WebClient?.IsBusy == true)
        {
            downloadItem.WebClient.CancelAsync();
        }

        AbortActiveRequest(downloadItem);
    }

    /// <summary>Resumes a paused item so the queue can download it again.</summary>
    public void ResumeItem(DownloadItem downloadItem)
    {
        downloadItem.IsPaused = false;
        downloadItem.ResetErrorState();
        lock (_urlsToDownload)
        {
            if (!_urlsToDownload.Contains(downloadItem))
            {
                _urlsToDownload.AddLast(downloadItem);
            }
        }
    }

    /// <summary>Removes an item from the download queue entirely (cancels a running download).</summary>
    public void RemoveItem(DownloadItem downloadItem)
    {
        downloadItem.Removed = true;
        downloadItem.IsPaused = false;
        if (downloadItem.WebClient?.IsBusy == true)
        {
            downloadItem.WebClient.CancelAsync();
        }

        AbortActiveRequest(downloadItem);

        lock (_urlsToDownload)
        {
            _ = _urlsToDownload.Remove(downloadItem);
        }
    }

    /// <summary>
    /// Requeues a failed item for another attempt (from the same mirror).
    /// Returns false when the item is still downloading or already removed.
    /// </summary>
    public bool RetryItem(DownloadItem downloadItem)
    {
        if (downloadItem.Removed || downloadItem.IsPaused || downloadItem.IsDownloading)
        {
            return false;
        }

        downloadItem.ResetTransferState();
        downloadItem.DownloadSlotStatus = null;
        lock (_urlsToDownload)
        {
            _urlsToDownload.AddLast(downloadItem);
        }
        return true;
    }

    /// <summary>
    /// Manually switches a queued item to the next mirror candidate (wrapping around to the first)
    /// and requeues it. Returns false when the item has no mirror candidates or is downloading.
    /// </summary>
    public bool SwitchMirror(DownloadItem downloadItem)
    {
        if (downloadItem.Candidates is not { Count: > 0 } candidates || downloadItem.IsDownloading)
        {
            return false;
        }

        downloadItem.CurrentMirrorIndex = (downloadItem.CurrentMirrorIndex + 1) % candidates.Count;
        downloadItem.ResetTransferState(); // different mirror -> start over
        downloadItem.DownloadSlotStatus = $"Switching to {candidates[downloadItem.CurrentMirrorIndex].Name}...";
        lock (_urlsToDownload)
        {
            _urlsToDownload.AddLast(downloadItem);
        }
        return true;
    }

    /// <summary>Items still waiting in the queue (for transferring them to a new download source).</summary>
    public List<DownloadItem> GetPendingItems()
    {
        List<DownloadItem> items;
        lock (_urlsToDownload)
        {
            items = [.. _urlsToDownload];
        }

        return [.. items.Where(i => !i.Removed && !i.IsCompleted)];
    }

    /// <summary>Adds an item to the download queue (used when migrating items to a new downloader).</summary>
    public void EnqueueItem(DownloadItem downloadItem)
    {
        lock (_urlsToDownload)
        {
            _urlsToDownload.AddLast(downloadItem);
        }
    }

    internal class FileWorkerArgs
    {
        public string action { get; set; }
        public string orginalLocation { get; set; }
        public string desiredLocation { get; set; }
    }
    protected virtual void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
    {
        lock (_lockingObject)
        {
            DownloadItem url = (DownloadItem)e.UserState;
            url.DownloadSpeed = 0;
            bool error = false;
            if (e.Cancelled)
            {
                // Pause / global stop / stall / remove — never lose the partial file.
                // Progress is remembered so the next attempt resumes from the byte offset
                // (the actual temp file size, which is what the server can resume from).
                string tempFileLocation = GetFullTempLocation(url.FileName);
                url.ResumeOffset = File.Exists(tempFileLocation) ? new FileInfo(tempFileLocation).Length : 0;
                if (url.Removed)
                {
                    url.DownloadAborted = true;
                    error = true; // remove temp on the next watcher tick
                }
                else if (url.IsPaused)
                {
                    // individually paused: stays out of the queue until ResumeItem()
                    url.DownloadAborted = false;
                }
                else
                {
                    // global stop / stall / source switch: stays queued and restarts when resumed
                    url.DownloadAborted = false;
                    lock (_urlsToDownload)
                    {
                        _ = _urlsToDownload.AddFirst(url);
                    }
                }
            }
            else if (e.Error != null)
            {
                if (TrySwitchMirror(url))
                {
                    return;
                }

                bool handled = false;
                if (e.Error is WebException ex && ex.Response is HttpWebResponse response)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        //deleted download
                        url.Error = "This beatmap is not available for download";
                        handled = error = true;
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        if (!TrySwitchMirror(url))
                        {
                            // official source: legacy 10 minute rate limit pause
                            url.OtherError = true;
                            url.Error = "Download limit hit - download has been paused (next check in 10minutes)";
                            if (!StopDownloads)
                            {
                                StopDownloads = true;
                                _ = Task.Run(async () =>
                                {
                                    await Task.Delay(60 * 1000 * 10);
                                    StopDownloads = false;
                                });
                            }

                            lock (_urlsToDownload)
                            {
                                _ = _urlsToDownload.AddFirst(url);
                            }
                        }

                        handled = true;
                    }
                }

                if (!handled)
                {
                    url.OtherError = true;
                    url.Error = "Fatal error: " + e.Error;
                    error = true;
                }
            }

            bool success = !e.Cancelled && e.Error == null;
            if (error)
            {
                string tempFileLocation = GetFullTempLocation(url.FileName);
                FileOperations.Enqueue(new FileWorkerArgs()
                {
                    action = "removeTemp",
                    orginalLocation = tempFileLocation
                });
            }
            else if (success)
            {
                url.DownloadSlotStatus = null;
                string tempFileLocation = GetFullTempLocation(url.FileName);
                string fileLocation = GetFullLocation(url.FileName);
                FileOperations.Enqueue(new FileWorkerArgs()
                {
                    action = "moveTemp",
                    orginalLocation = tempFileLocation,
                    desiredLocation = fileLocation
                });

            }

            downloadCheck[url.WebClient.ClientId].Reset();
            Clients.Enqueue(url.WebClient);
        }
    }

    private string GetFullLocation(string filename) => Path.Combine(_saveLocation, filename);
    private string GetFullTempLocation(string filename) => Path.Combine(_saveLocation, GetTempFilename(filename));
    private static string GetTempFilename(string path) => path + ".tmp";

    private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        int progress = e.ProgressPercentage;

        DownloadItem DlItem = (DownloadItem)e.UserState;
        DownloadProgress check = downloadCheck[DlItem.WebClient.ClientId];
        long now = Environment.TickCount64;
        if (check.LastTick != 0)
        {
            long elapsedMs = now - check.LastTick;
            long receivedDelta = e.BytesReceived - check.bytesRecived;
            if (elapsedMs > 0)
            {
                DlItem.DownloadSpeed = receivedDelta * 1000.0 / elapsedMs;
            }
        }

        check.LastTick = now;
        check.bytesRecived = e.BytesReceived;
        if (DlItem.lastShownDlState != progress)
        {
            DlItem.lastShownDlState = progress;
            OnProgressUpdated(e);
        }
    }
    public DownloadItem DownloadFile(string url, string filename, string referer, object token, int requestTimeout)
    {
        DownloadItem dlItem = new() { FileName = filename, Url = url, Referer = referer, UserToken = token, RequestTimeout = requestTimeout };
        lock (_urlsToDownload)
        {
            _ = _urlsToDownload.AddLast(dlItem);
        }

        return dlItem;
    }

    protected virtual void OnProgressUpdated(DownloadProgressChangedEventArgs e)
    {
        DownloadItem dlItem = (DownloadItem)e.UserState;
        dlItem.BytesRecived = e.BytesReceived;
        dlItem.TotalBytes = e.TotalBytesToReceive;
        dlItem.ProgressPrecentage = e.ProgressPercentage;
        ProgressUpdated?.Invoke(this, e);
    }

    public void Dispose()
    {
        _ProgressWatcher?.Dispose();
        _urlWatcher?.Dispose();

        GC.SuppressFinalize(this);
    }
}
