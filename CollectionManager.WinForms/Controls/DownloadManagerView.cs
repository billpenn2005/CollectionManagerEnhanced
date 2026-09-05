namespace GuiComponents.Controls;

using CollectionManager.Common.Interfaces;
using CollectionManager.Common.Interfaces.Controls;
using CollectionManager.Extensions.Modules.Downloader.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

public partial class DownloadManagerView : UserControl, IDownloadManagerView
{
    public DownloadManagerView()
    {
        InitializeComponent();

        button_ToggleDownloads.Click += (s, a) => DownloadToggleClick?.Invoke(this, EventArgs.Empty);
        ListViewDownload.FullRowSelect = true;
        ListViewDownload.MultiSelect = true;

        contextMenuStrip_downloads.Opening += ContextMenuStrip_Downloads_Opening;
        menuItem_pause.Click += (_, _) => ItemPauseRequested?.Invoke(this, EventArgs.Empty);
        menuItem_resume.Click += (_, _) => ItemResumeRequested?.Invoke(this, EventArgs.Empty);
        menuItem_remove.Click += (_, _) => ItemRemoveRequested?.Invoke(this, EventArgs.Empty);
        menuItem_retry.Click += (_, _) => ItemRetryRequested?.Invoke(this, EventArgs.Empty);
        menuItem_switchMirror.Click += (_, _) => ItemSwitchMirrorRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler DownloadToggleClick;
    public event EventHandler ItemPauseRequested;
    public event EventHandler ItemResumeRequested;
    public event EventHandler ItemRemoveRequested;
    public event EventHandler ItemRetryRequested;
    public event EventHandler ItemSwitchMirrorRequested;

    public bool DownloadButtonIsEnabled
    {
        get => button_ToggleDownloads.Enabled; set => button_ToggleDownloads.Enabled = value;
    }

    public string DownloadButtonText
    {
        set => button_ToggleDownloads.Text = value;
    }

    public IEnumerable<IDownloadItem> SelectedItems => ListViewDownload.SelectedObjects.Cast<IDownloadItem>();

    private void ContextMenuStrip_Downloads_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        IReadOnlyList<DownloadItem> selected = ListViewDownload.SelectedObjects.Cast<DownloadItem>().ToList();
        bool any = selected.Count > 0;
        bool anyActive = selected.Any(i => i.WebClient?.IsBusy == true);
        bool anyPaused = selected.Any(i => i.IsPaused);
        menuItem_pause.Enabled = any && anyActive;
        menuItem_resume.Enabled = any && anyPaused;
        menuItem_remove.Enabled = any;
        menuItem_retry.Enabled = any && selected.Any(i => i.OtherError || i.DownloadAborted);
        menuItem_switchMirror.Enabled = any && selected.Any(i => i.Candidates is { Count: > 0 }) && !anyActive;
    }

    private void SafeInvoke(MethodInvoker action)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(action);
            }
            catch (InvalidOperationException)
            {
                // control destroyed mid-update
            }
        }
        else
        {
            action();
        }
    }

    public void SetDownloadItems(ICollection<IDownloadItem> downloadItems)
    {
        SafeInvoke(() =>
        {
            if (IsDisposed)
            {
                return;
            }

            ListViewDownload.SetObjects(downloadItems.ToList());
        });
    }

    public void UpdateDownloadItem(IDownloadItem downloadItem)
    {
        SafeInvoke(() =>
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                ListViewDownload.RefreshObject(downloadItem);
            }
            catch
            {
                // item may have been removed in the meantime
            }
        });
    }
}