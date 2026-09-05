namespace CollectionManager.App.Shared.Presenters.Forms;

using CollectionManager.App.Shared.Misc;
using CollectionManager.App.Shared.Models;
using CollectionManager.Common;
using CollectionManager.Common.Interfaces;
using CollectionManager.Common.Interfaces.Forms;
using CollectionManager.Extensions.Modules.Downloader.Mirrors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Presenter for the in-app download source manager: list, edit and reorder
/// download sources / anonymous mirrors, then persist them into the program settings.
/// </summary>
public sealed class DownloadSourcesPresenter
{
    private readonly IDownloadSourcesForm _view;
    private readonly IUserDialogs _userDialogs;
    private List<DownloadSource> _sources;

    public DownloadSourcesPresenter(IDownloadSourcesForm view, IUserDialogs userDialogs)
    {
        _view = view;
        _userDialogs = userDialogs;

        _sources = OsuDownloadManager.Instance.GetDownloadSources().OfType<DownloadSource>().ToList();

        _view.SelectedSourceChanged += (_, _) => RefreshSource();
        _view.MirrorSelectionChanged += (_, _) => RefreshMirrorEdit();
        _view.AddMirrorRequested += (_, _) => AddMirror();
        _view.RemoveMirrorRequested += (_, _) => RemoveMirror();
        _view.MoveMirrorUpRequested += (_, _) => MoveMirror(-1);
        _view.MoveMirrorDownRequested += (_, _) => MoveMirror(1);
        _view.SaveRequested += async (_, _) => await SaveAsync();

        _view.SetSources(_sources.Select(s => s.Name).ToList());
        _view.Show();
    }

    private DownloadSource SelectedSource => _view.SelectedSourceIndex >= 0 && _view.SelectedSourceIndex < _sources.Count
        ? _sources[_view.SelectedSourceIndex]
        : null;

    private static List<DownloadSourceMirror> GetMirrorList(DownloadSource source) => source.Mirrors ??= [];

    private void RefreshSource()
    {
        DownloadSource source = SelectedSource;
        if (source is null)
        {
            _view.SetSourceInfo(string.Empty);
            _view.SetMirrors([]);
            _view.SetMirrorEditEnabled(false);
            return;
        }

        _view.SetSourceInfo(
            $"Name: {source.Name}{Environment.NewLine}" +
            $"Requires login: {(source.RequiresLogin ? (source.UseCookiesLogin ? "cookies" : "username/password") : "no")}{Environment.NewLine}" +
            $"Throttle: {(source.ThrottleDownloads ? $"{source.DownloadsPerMinute}/minute, {source.DownloadsPerHour}/hour" : "none")}{Environment.NewLine}" +
            $"Download threads: {source.DownloadThreads}");

        if (GetMirrorList(source).Count > 0)
        {
            _view.SetMirrors(GetMirrorList(source).Select(ToEdit).ToList());
            _view.SetMirrorEditEnabled(true);
        }
        else
        {
            _view.SetMirrors([]);
            _view.SetMirrorEditEnabled(false);
        }
    }

    private static DownloadSourceMirrorEdit ToEdit(DownloadSourceMirror mirror) => new()
    {
        Name = mirror.Name,
        Url = mirror.TemplateUrl,
        UrlNoVideo = mirror.TemplateUrlNoVideo,
        Referer = mirror.Referer
    };

    private void RefreshMirrorEdit()
    {
        if (_view.SelectedMirrorIndex < 0)
        {
            return;
        }

        List<DownloadSourceMirror> mirrors = GetMirrorList(SelectedSource);
        if (mirrors is null || _view.SelectedMirrorIndex >= mirrors.Count)
        {
            return;
        }

        _view.SetMirrorEdit(ToEdit(mirrors[_view.SelectedMirrorIndex]));
    }

    private void AddMirror()
    {
        DownloadSource source = SelectedSource;
        if (source is null)
        {
            return;
        }

        GetMirrorList(source).Add(new DownloadSourceMirror { Name = "New mirror", TemplateUrl = "https://example.com/d/{0}", TemplateUrlNoVideo = "https://example.com/d/{0}", Referer = string.Empty });
        RefreshSource();
        _view.SelectedMirrorIndex = GetMirrorList(source).Count - 1;
        RefreshMirrorEdit();
    }

    private void RemoveMirror()
    {
        DownloadSource source = SelectedSource;
        if (source is null || _view.SelectedMirrorIndex < 0 || _view.SelectedMirrorIndex >= GetMirrorList(source).Count)
        {
            return;
        }

        GetMirrorList(source).RemoveAt(_view.SelectedMirrorIndex);
        RefreshSource();
    }

    private void MoveMirror(int direction)
    {
        DownloadSource source = SelectedSource;
        List<DownloadSourceMirror> mirrors = GetMirrorList(source);
        int index = _view.SelectedMirrorIndex;
        int target = index + direction;
        if (source is null || index < 0 || target < 0 || target >= mirrors.Count)
        {
            return;
        }

        (mirrors[index], mirrors[target]) = (mirrors[target], mirrors[index]);
        RefreshSource();
        _view.SelectedMirrorIndex = target;
    }

    private async Task SaveAsync()
    {
        // write the currently selected mirror's edited fields back
        DownloadSource source = SelectedSource;
        if (source is not null && _view.SelectedMirrorIndex >= 0 && _view.SelectedMirrorIndex < GetMirrorList(source).Count)
        {
            GetMirrorList(source)[_view.SelectedMirrorIndex] = new DownloadSourceMirror
            {
                Name = _view.MirrorName,
                TemplateUrl = _view.MirrorUrl,
                TemplateUrlNoVideo = _view.MirrorUrlNoVideo,
                Referer = _view.MirrorReferer
            };
        }

        OsuDownloadManager.Instance.SaveDownloadSources(_sources);
        await _userDialogs.OkMessageBoxAsync("Download sources saved.", "Download sources", MessageBoxType.Success);
    }
}