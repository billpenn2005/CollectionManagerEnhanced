namespace CollectionManager.App.Shared.Interfaces.Controls;

using CollectionManager.Core.Modules.Collection;
using CollectionManager.Core.Types;

public interface ICollectionListingModel
{
    event EventHandler CollectionsChanged;
    event EventHandler SelectedCollectionsChanged;
    event EventHandler<CollectionEditArgs> CollectionEditing;
    OsuCollections GetCollections();
    OsuCollections SelectedCollections { get; set; }

    void EmitCollectionEditing(CollectionEditArgs args);
    /// <summary>Re-emits the collections changed event so views refresh (e.g. after downloads changed missing states).</summary>
    void RefreshCollections();
    OsuCollections GetCollectionsForBeatmaps(Beatmaps beatmaps);

}