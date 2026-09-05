namespace CollectionManager.Common.Interfaces.Forms;

using System;
using System.Collections.Generic;

/// <summary>
/// Mirror entry shown in the "Download sources" settings window.
/// </summary>
public struct DownloadSourceMirrorEdit
{
    public string Name;
    public string Url;
    public string UrlNoVideo;
    public string Referer;
}

/// <summary>View contract for the in-app download source / mirror management window.</summary>
public interface IDownloadSourcesForm : IForm
{
    event EventHandler SelectedSourceChanged;
    event EventHandler MirrorSelectionChanged;
    event EventHandler AddMirrorRequested;
    event EventHandler RemoveMirrorRequested;
    event EventHandler MoveMirrorUpRequested;
    event EventHandler MoveMirrorDownRequested;
    event EventHandler SaveRequested;

    int SelectedSourceIndex { get; }
    int SelectedMirrorIndex { get; set; }
    string MirrorName { get; }
    string MirrorUrl { get; }
    string MirrorUrlNoVideo { get; }
    string MirrorReferer { get; }

    void SetSources(IReadOnlyList<string> names);
    void SetSourceInfo(string info);
    void SetMirrors(IReadOnlyList<DownloadSourceMirrorEdit> mirrors);
    void SetMirrorEdit(DownloadSourceMirrorEdit mirror);
    void SetMirrorEditEnabled(bool enabled);
    void SetSaveEnabled(bool enabled);
}