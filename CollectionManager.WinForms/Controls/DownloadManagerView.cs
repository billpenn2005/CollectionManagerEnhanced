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

        comboBox_source.SelectedIndexChanged += (_, _) => DownloadSourceChanged?.Invoke(this, EventArgs.Empty);

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
    public event EventHandler DownloadSourceChanged;

    public bool DownloadButtonIsEnabled
    {
        get => button_ToggleDownloads.Enabled; set => button_ToggleDownloads.Enabled = value;
    }

    public string DownloadButtonText
    {
        set => button_ToggleDownloads.Text = value;
    }

    public string SelectedDownloadSourceName => comboBox_source.SelectedItem as string;

    public void SetDownloadSources(IEnumerable<string> names, string selected)
    {
        comboBox_source.Items.Clear();
        comboBox_source.Items.AddRange(names.Cast<object>().ToArray());
        int index = Math.Max(0, names.ToList().IndexOf(selected ?? string.Empty));
        comboBox_source.SelectedIndex = index;
    }

    public IEnumerable<IDownloadItem> SelectedItems => ListViewDownload.SelectedObjects.Cast<IDownloadItem>();

    private void ContextMenuStrip_Downloads_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        IReadOnlyList<DownloadItem> selected = ListViewDownload.SelectedObjects.Cast<DownloadItem>().ToList();
        bool any = selected.Count > 0;
        bool anyCompleted = selected.Any(i => i.Removed || i.IsCompleted);
        menuItem_pause.Enabled = any && selected.Any(i => !i.Removed && !i.IsCompleted && !i.IsPaused);
        menuItem_resume.Enabled = any && selected.Any(i => i.IsPaused);
        menuItem_remove.Enabled = any;
        menuItem_retry.Enabled = any && selected.Any(i => !i.IsPaused && (i.OtherError || i.DownloadAborted));
        menuItem_switchMirror.Enabled = any && !anyCompleted && selected.Any(i => i.Candidates is { Count: > 0 }) && selected.All(i => i.WebClient?.IsBusy != true && !i.IsPaused);
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