namespace CollectionManager.App.Shared.Presenters.Forms;

using CollectionManager.App.Shared.Models.Forms;
using CollectionManager.Common;
using CollectionManager.Common.Interfaces;
using CollectionManager.Common.Interfaces.Forms;
using CollectionManager.Core.Extensions;
using CollectionManager.Core.Types;
using CollectionManager.Extensions.Modules.MergedOsz;
using CollectionManager.Extensions.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class MergedOszExportFormPresenter
{
    private readonly IMergedOszExportForm _view;
    private readonly MergedOszExportModel _model;
    private readonly IUserDialogs _userDialogs;

    public MergedOszExportFormPresenter(IMergedOszExportForm view, MergedOszExportModel model, IUserDialogs userDialogs)
    {
        _view = view;
        _model = model;
        _userDialogs = userDialogs;

        _model.Collections = Initalizer.LoadedCollections;
        _view.SetCollections(_model.Collections);

        _view.SelectedCollectionChanged += (_, _) => OnSelectedCollectionChanged();
        _view.MoveToExportClicked += (_, _) => MoveToExport();
        _view.MoveBackClicked += (_, _) => MoveBack();
        _view.MoveUpClicked += (_, _) => MoveSelected(-1);
        _view.MoveDownClicked += (_, _) => MoveSelected(1);
        _view.RenameRequested += (_, args) => Rename(args);
        _view.ExportClicked += async (_, _) => await ExportAsync();
        _view.BeatmapsDroppedToExport += (_, beatmaps) => AddBeatmaps(beatmaps);
        _view.ExportItemsDroppedBack += (_, items) => RemoveItems(items);
    }

    private void OnSelectedCollectionChanged()
    {
        _model.SelectedCollection = _view.SelectedCollection;

        if (_model.SelectedCollection is null)
        {
            _model.SourceBeatmaps = null;
            _view.SetSourceBeatmaps(null);
            return;
        }

        _model.SourceBeatmaps = new Beatmaps(_model.SelectedCollection.AllBeatmaps());
        _view.SetSourceBeatmaps(_model.SourceBeatmaps);
    }

    private void MoveToExport()
    {
        if (_view.SelectedSourceBeatmaps is null)
        {
            return;
        }

        int missingCount = _view.SelectedSourceBeatmaps.Count(beatmap => beatmap is BeatmapExtension extension && extension.LocalBeatmapMissing);

        if (missingCount != 0)
        {
            _ = _userDialogs.OkMessageBoxAsync($"{missingCount} beatmap(s) without local files were skipped. Only beatmaps present in your osu! folder can be packed.", "Export merged osz", MessageBoxType.Info);
        }

        AddBeatmaps(new Beatmaps(_view.SelectedSourceBeatmaps.Where(beatmap => beatmap is not BeatmapExtension extension || !extension.LocalBeatmapMissing)));
    }

    private void AddBeatmaps(IEnumerable<Beatmap> beatmaps)
    {
        bool anyAdded = false;

        foreach (Beatmap beatmap in beatmaps)
        {
            if (_model.ExportItems.Any(item => !item.IsPlaceholder && item.Beatmap.Hash == beatmap.Hash))
            {
                continue;
            }

            _model.ExportItems.Add(MergedOszBeatmap.FromBeatmap(beatmap));
            anyAdded = true;
        }

        if (anyAdded)
        {
            _view.SetExportItems(_model.ExportItems);
        }
    }

    private void MoveBack()
    {
        if (_view.SelectedExportItems is null || _view.SelectedExportItems.Count is 0)
        {
            return;
        }

        RemoveItems(_view.SelectedExportItems);
    }

    private void RemoveItems(IEnumerable<MergedOszBeatmap> items)
    {
        bool anyRemoved = false;

        foreach (MergedOszBeatmap item in items)
        {
            if (item.IsPlaceholder)
            {
                continue;
            }

            anyRemoved |= _model.ExportItems.Remove(item);
        }

        if (anyRemoved)
        {
            _view.SetExportItems(_model.ExportItems);
        }
    }

    private void MoveSelected(int direction)
    {
        MergedOszBeatmap item = _view.SelectedExportItems?.FirstOrDefault();

        if (item is null)
        {
            return;
        }

        int index = _model.ExportItems.IndexOf(item);

        // Index 0 is the placeholder, nothing can be moved before it
        if (index < 0 || index + direction < 1 || index + direction >= _model.ExportItems.Count)
        {
            return;
        }

        (_model.ExportItems[index], _model.ExportItems[index + direction]) = (_model.ExportItems[index + direction], _model.ExportItems[index]);
        _view.SetExportItems(_model.ExportItems);
    }

    private void Rename(MergedOszRenameRequestEventArgs args)
    {
        if (args.Index < 0 || args.Index >= _model.ExportItems.Count)
        {
            return;
        }

        _model.ExportItems[args.Index].DisplayName = string.IsNullOrWhiteSpace(args.NewName) ? null : args.NewName.Trim();
        _view.SetExportItems(_model.ExportItems);
    }

    private async Task ExportAsync()
    {
        string packName = _view.PackName?.Trim();
        string creator = _view.Creator?.Trim();
        string outputDirectory = _view.OutputDirectory?.Trim();

        if (string.IsNullOrWhiteSpace(packName))
        {
            await _userDialogs.OkMessageBoxAsync("Pack name is required.", "Export merged osz", MessageBoxType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(creator))
        {
            await _userDialogs.OkMessageBoxAsync("Creator name is required.", "Export merged osz", MessageBoxType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            await _userDialogs.OkMessageBoxAsync("Output directory is required.", "Export merged osz", MessageBoxType.Error);
            return;
        }

        if (!Directory.Exists(outputDirectory))
        {
            await _userDialogs.OkMessageBoxAsync("Output directory does not exist.", "Export merged osz", MessageBoxType.Error);
            return;
        }

        if (_model.ExportItems.Count <= 1)
        {
            await _userDialogs.OkMessageBoxAsync("Add at least one beatmap to the export list.", "Export merged osz", MessageBoxType.Error);
            return;
        }

        MergedOszExporter exporter = new(BeatmapUtils.OsuSongsDirectory, outputDirectory);

        Progress<string> stringProgress = new();
        Progress<int> percentageProgress = new();

        using CancellationTokenSource cancellationTokenSource = new();
        IProgressForm progressForm = await _userDialogs.CreateProgressFormAsync(stringProgress, percentageProgress);
        progressForm.AbortClicked += (_, _) =>
        {
            if (!cancellationTokenSource.TryCancel())
            {
                progressForm.Close();
            }
        };

        progressForm.Show();

        try
        {
            IReadOnlyList<MergedOszExporter.FailedExport> failedExports = await Task.Run(() =>
                exporter.Export(_model.ExportItems.ToList(), packName, creator, _view.ExtraTags?.Trim(), stringProgress, percentageProgress, cancellationTokenSource.Token));

            string oszPath = Path.Combine(outputDirectory, $"{SanitizeFileName(packName)}.osz");
            int totalCount = _model.ExportItems.Count;

            if (failedExports.Count is 0)
            {
                await _userDialogs.OkMessageBoxAsync($"Exported {totalCount} beatmaps.{Environment.NewLine}{Environment.NewLine}.osz file:{Environment.NewLine}{oszPath}{Environment.NewLine}{Environment.NewLine}Song list (bbcode):{Environment.NewLine}{Path.Combine(outputDirectory, "bbcode.txt")}", "Export merged osz");
            }
            else
            {
                File.WriteAllText(Path.Combine(outputDirectory, "log.txt"), CreateErrorLog(failedExports));
                await _userDialogs.OkMessageBoxAsync($"Exported {totalCount - failedExports.Count} of {totalCount} beatmaps.{Environment.NewLine}{failedExports.Count} beatmap(s) failed, see log.txt in the output directory.", "Export merged osz", MessageBoxType.Error);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            await _userDialogs.OkMessageBoxAsync($"Error occurred during export: {exception}", "Export merged osz", MessageBoxType.Error);
        }
        finally
        {
            progressForm.Close();
        }
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalidChars.Contains(c) ? '_' : c));
    }

    private static string CreateErrorLog(IReadOnlyList<MergedOszExporter.FailedExport> failedExports)
    {
        StringBuilder stringBuilder = new();

        foreach (MergedOszExporter.FailedExport failedExport in failedExports)
        {
            _ = stringBuilder.AppendFormat("\"{0}\" failed with exception: {1}{2}", failedExport.Item.UiDisplayName, failedExport.Error, Environment.NewLine);
        }

        return stringBuilder.ToString();
    }
}