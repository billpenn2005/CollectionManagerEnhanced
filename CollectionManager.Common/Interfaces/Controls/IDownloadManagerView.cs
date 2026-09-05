namespace CollectionManager.Common.Interfaces.Controls;

using System;
using System.Collections.Generic;

public interface IDownloadManagerView
{
    event EventHandler DownloadToggleClick;
    event EventHandler ItemPauseRequested;
    event EventHandler ItemResumeRequested;
    event EventHandler ItemRemoveRequested;
    event EventHandler ItemRetryRequested;
    event EventHandler ItemSwitchMirrorRequested;
    event EventHandler DownloadSourceChanged;
    event EventHandler Disposed;

    bool DownloadButtonIsEnabled { set; }
    string DownloadButtonText { set; }
    string SelectedDownloadSourceName { get; }
    IEnumerable<IDownloadItem> SelectedItems { get; }
    void SetDownloadSources(IEnumerable<string> names, string selected);
    void SetDownloadItems(ICollection<IDownloadItem> downloadItems);
    void UpdateDownloadItem(IDownloadItem downloadItem);
}