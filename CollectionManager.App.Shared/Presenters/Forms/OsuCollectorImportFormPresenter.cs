namespace CollectionManager.App.Shared.Presenters.Forms;

using CollectionManager.App.Shared;
using CollectionManager.App.Shared.Misc;
using CollectionManager.App.Shared.Misc.SidePanelActions;
using CollectionManager.Common;
using CollectionManager.Common.Interfaces;
using CollectionManager.Common.Interfaces.Forms;
using CollectionManager.Core.Modules.Collection;
using CollectionManager.Core.Types;
using CollectionManager.Extensions.Modules.API.OsuCollector;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Presenter for importing a collection from osucollector.com by link or ID.
/// </summary>
public sealed class OsuCollectorImportFormPresenter
{

    private readonly IOsuCollectorImportForm _view;
    private readonly IUserDialogs _userDialogs;
    private readonly ILoginFormView _loginForm;
    private readonly OsuCollectorApi _api;

    public OsuCollectorImportFormPresenter(IOsuCollectorImportForm view, IUserDialogs userDialogs, ILoginFormView loginForm, OsuCollectorApi api = null)
    {
        _view = view;
        _userDialogs = userDialogs;
        _loginForm = loginForm;
        _api = api ?? new OsuCollectorApi();

        _view.ImportAfterDownloadBehavior = Initalizer.Settings.OsuCollectorImportDownloadBehavior;
        _view.BehaviorChanged += (_, _) => SaveBehaviorSetting();
        _view.ImportClicked += async (_, _) => await ImportAsync();
        _view.Show();
    }

    private void SaveBehaviorSetting()
        => Initalizer.Settings.OsuCollectorImportDownloadBehavior = _view.ImportAfterDownloadBehavior;

    private async Task ImportAsync()
    {
        if (!int.TryParse(_view.CollectionLink?.Trim(), out int id))
        {
            try
            {
                id = OsuCollectorApi.ParseCollectionId(_view.CollectionLink);
            }
            catch (ArgumentException exception)
            {
                await _userDialogs.OkMessageBoxAsync(exception.Message, "Import from osu!collector", MessageBoxType.Error);
                return;
            }
        }

        if (OsuCollectorApi.IsAlreadyImported(id, Initalizer.LoadedCollections))
        {
            await _userDialogs.OkMessageBoxAsync(
                $"Collection {id} has already been imported and is present in the collection listing.",
                "Import from osu!collector", MessageBoxType.Info);
            return;
        }

        _view.SetImportingState(true);
        try
        {
            OsuCollectorCollection collection = await _api.GetCollectionAsync(id);
            OsuCollection osuCollection = _api.ToOsuCollection(collection, Initalizer.OsuFileIo.LoadedMaps);

            Initalizer.CollectionsManager.EditCollection(CollectionEditArgs.AddCollections(new OsuCollections { osuCollection }));

            _view.SetResultSummary(
                $"Name: {collection.Name}{Environment.NewLine}" +
                $"Uploader: {collection.Uploader?.Username}{Environment.NewLine}" +
                $"Beatmaps: {collection.BeatmapCount}{Environment.NewLine}" +
                (string.IsNullOrEmpty(collection.Description) ? string.Empty : $"Description: {collection.Description}"));
            await _userDialogs.OkMessageBoxAsync(
                $"Collection \"{collection.Name}\" imported ({collection.BeatmapCount} beatmaps).",
                "Import from osu!collector", MessageBoxType.Success);

            await HandleDownloadAfterImportAsync();
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            await _userDialogs.OkMessageBoxAsync($"Failed to fetch the collection: {exception.Message}", "Import from osu!collector", MessageBoxType.Error);
        }
        finally
        {
            _view.SetImportingState(false);
        }
    }

    private async Task HandleDownloadAfterImportAsync()
    {
        string behavior = _view.ImportAfterDownloadBehavior;

        if (behavior == OsuCollectorImportBehaviors.DoNothing)
        {
            return;
        }

        bool startDownload = behavior == OsuCollectorImportBehaviors.DownloadDirectly;
        if (!startDownload)
        {
            (startDownload, bool doNotAskAgain) = await _userDialogs.YesNoMessageBoxAsync(
                "Download the maps missing from this collection now?", "Import from osu!collector",
                MessageBoxType.Question, "Don't ask again");
            if (doNotAskAgain)
            {
                Initalizer.Settings.OsuCollectorImportDownloadBehavior = startDownload ? OsuCollectorImportBehaviors.DownloadDirectly : OsuCollectorImportBehaviors.DoNothing;
            }
        }

        if (startDownload)
        {
            if (await OsuDownloadManager.Instance.AskUserForSaveDirectoryAndLoginAsync(_userDialogs, _loginForm))
            {
                Beatmaps downloadable = [.. Initalizer.LoadedCollections
                    .Where(c => c.OnlineId > 0)
                    .SelectMany(c => c.DownloadableBeatmaps)];
                if (downloadable.Count > 0)
                {
                    OsuDownloadManager.Instance.DownloadBeatmaps(downloadable);
                    ShowDownloadManagerHandler.Instance.ShowDownloadManager();
                }
                else
                {
                    await _userDialogs.OkMessageBoxAsync("No missing maps to download.", "Import from osu!collector", MessageBoxType.Info);
                }
            }
        }
    }
}