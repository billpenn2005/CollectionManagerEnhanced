namespace CollectionManager.App.Shared.Presenters.Forms;

using CollectionManager.App.Shared;
using CollectionManager.App.Shared.Interfaces.Controls;
using CollectionManager.App.Shared.Interfaces.Forms;
using CollectionManager.App.Shared.Models.Controls;
using CollectionManager.App.Shared.Misc;
using CollectionManager.App.Shared.Presenters.Controls;
using CollectionManager.Common.Interfaces.Forms;
using CollectionManager.Core.Modules.Collection;
using CollectionManager.Core.Modules.FileIo;
using CollectionManager.Core.Types;
using CollectionManager.Extensions.Modules.Downloader.Api;
using CollectionManager.Extensions.Utils;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class MainFormPresenter
{
    private readonly IMainFormView _view;
    private readonly IMainFormModel _mainFormModel;
    private readonly ICollectionTextModel _collectionTextModel;
    private readonly ICombinedBeatmapPreviewModel _combinedBeatmapPreviewModel;
    public IInfoTextModel InfoTextModel;
    public readonly IBeatmapListingModel BeatmapListingModel;
    public readonly ICollectionListingModel CollectionListingModel;
    private readonly IScoresListingModel _scoresListingModel;
    private readonly object _downloadProcessingLock = new();
    public MainFormPresenter(IMainFormView view, IMainFormModel mainFormModel, IInfoTextModel infoTextModel, IWebCollectionProvider webCollectionProvider)
    {
        _view = view;
        _mainFormModel = mainFormModel;

        //Combined listing (Collections+Beatmaps)
        BeatmapListingModel = new BeatmapListingModel(null);
        BeatmapListingModel.BeatmapsDropped += BeatmapListing_BeatmapsDropped;
        BeatmapListingModel.SelectedBeatmapChanged += BeatmapListingViewOnSelectedBeatmapChanged;
        CollectionListingModel = new CollectionListingModel(Initalizer.LoadedCollections, _mainFormModel.GetCollectionEditor());
        CollectionListingModel.CollectionEditing += CollectionListing_CollectionEditing;
        CollectionListingModel.SelectedCollectionsChanged += CollectionListing_SelectedCollectionsChanged;
        _ = new CombinedListingPresenter(_view.CombinedListingView, CollectionListingModel, BeatmapListingModel, webCollectionProvider, mainFormModel.GetUserDialogs());

        //Beatmap preview stuff (images, beatmap info like ar,cs,stars...)
        _combinedBeatmapPreviewModel = new CombinedBeatmapPreviewModel();
        CombinedBeatmapPreviewPresenter presenter = new(_view.CombinedBeatmapPreviewView, _combinedBeatmapPreviewModel);

        presenter.MusicControlModel.NextMapRequest += (s, a) => _view.CombinedListingView.beatmapListingView.SelectNextOrFirst();

        _collectionTextModel = new CollectionTextModel();
        _ = new CollectionTextPresenter(_view.CollectionTextView, _collectionTextModel);

        _scoresListingModel = new ScoresListingModel();
        _ = new ScoresListingPresenter(_view.ScoresListingView, _scoresListingModel);

        //General information (Collections loaded, update check etc.)
        InfoTextModel = infoTextModel;
        _ = new InfoTextPresenter(_view.InfoTextView, InfoTextModel);

        // downloads that complete are unpacked into the osu! songs folder and collections
        // are refreshed so the beatmaps stop showing as missing
        OsuDownloadManager.Instance.DownloadFinished += (_, _) => _ = Task.Run(ProcessFinishedDownloads);
    }

    /// <summary>
    /// Unpacks completed downloads into <see cref="BeatmapUtils.OsuSongsDirectory"/> and registers the
    /// parsed beatmaps in the map cache (which re-evaluates each collection's missing state).
    /// Runs on a background thread; the final view refresh is marshalled to the UI thread.
    /// </summary>
    private void ProcessFinishedDownloads()
    {
        if (!Monitor.TryEnter(_downloadProcessingLock))
        {
            return; // another run is already unpacking
        }

        try
        {
            List<DownloadItem> completed = OsuDownloadManager.Instance.DownloadItems.OfType<DownloadItem>()
                .Where(i => !i.Removed && i.IsCompleted && !string.IsNullOrEmpty(OsuDownloadManager.Instance.DownloadDirectory))
                .ToList();
            if (completed.Count is 0)
            {
                return;
            }

            List<BeatmapExtension> parsed = [];
            foreach (DownloadItem item in completed)
            {
                string oszPath = Path.Combine(OsuDownloadManager.Instance.DownloadDirectory, item.FileName);
                if (!File.Exists(oszPath))
                {
                    continue;
                }

                try
                {
                    parsed.AddRange(UnpackMapset(oszPath));
                }
                catch
                {
                    // corrupt/partial osz: leave it to the user
                }
            }

            if (parsed.Count is 0)
            {
                return;
            }

            _view.InvokeOnUIThread(() => RegisterDownloadedBeatmaps(parsed));
        }
        finally
        {
            Monitor.Exit(_downloadProcessingLock);
        }
    }

    /// <summary>Extracts the .osz into the osu! songs folder and parses its .osu files.</summary>
    private static List<BeatmapExtension> UnpackMapset(string oszPath)
    {
        string songsDirectory = BeatmapUtils.OsuSongsDirectory;
        if (string.IsNullOrEmpty(songsDirectory) || !Directory.Exists(songsDirectory))
        {
            return [];
        }

        string targetDirectory = Path.Combine(songsDirectory, Path.GetFileNameWithoutExtension(oszPath));
        Directory.CreateDirectory(targetDirectory);
        ZipFile.ExtractToDirectory(oszPath, targetDirectory, overwriteFiles: true);

        return Directory.GetFiles(targetDirectory, "*.osu")
            .Select(BeatmapFileParser.Parse)
            .Where(b => b.MapId > 10 || !string.IsNullOrEmpty(b.Title))
            .ToList();
    }

    /// <summary>Registers parsed beatmaps in the map cache and refreshes the collection/beatmap views. UI thread.</summary>
    private void RegisterDownloadedBeatmaps(List<BeatmapExtension> parsed)
    {
        bool anyRegistered = false;
        foreach (BeatmapExtension beatmap in parsed)
        {
            if (beatmap.MapId > 10 && Initalizer.OsuFileIo.LoadedMaps.GetByMapId(beatmap.MapId) is not null)
            {
                continue; // already known
            }

            Initalizer.OsuFileIo.LoadedMaps.StoreBeatmap(beatmap);
            anyRegistered = true;
        }

        if (!anyRegistered)
        {
            return;
        }

        // refresh the collection list (missing counts) and the current collection's beatmap list
        CollectionListingModel.RefreshCollections();
        if (BeatmapListingModel.CurrentCollection is not null)
        {
            BeatmapListingModel.SetCollection(BeatmapListingModel.CurrentCollection);
        }
    }

    private void CollectionListing_CollectionEditing(object sender, CollectionEditArgs collectionEditArgs) => _mainFormModel.GetCollectionEditor()?.EditCollection(collectionEditArgs);

    private void CollectionListing_SelectedCollectionsChanged(object sender, EventArgs eventArgs)
    {
        OsuCollections collections = CollectionListingModel.SelectedCollections;
        if (collections != null)
        {
            _collectionTextModel.SetCollections(collections);
        }
    }

    private void BeatmapListing_BeatmapsDropped(object sender, Beatmaps args)
    {
        if (CollectionListingModel.SelectedCollections?.Count == 1)
        {
            IOsuCollection collection = CollectionListingModel.SelectedCollections[0];
            CollectionListing_CollectionEditing(sender, CollectionEditArgs.AddBeatmaps(collection.Name, args));
        }
    }

    private void BeatmapListingViewOnSelectedBeatmapChanged(object sender, EventArgs eventArgs)
    {
        Beatmap selectedBeatmap = _view.CombinedListingView.beatmapListingView.SelectedBeatmap;
        _combinedBeatmapPreviewModel.SetBeatmap(selectedBeatmap);

        Scores scores = selectedBeatmap is null ? null : Initalizer.OsuFileIo.ScoresDatabase.GetScores(selectedBeatmap);
        _scoresListingModel.Scores = scores;
    }
}