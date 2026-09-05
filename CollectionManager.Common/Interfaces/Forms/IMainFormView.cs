namespace CollectionManager.Common.Interfaces.Forms;
using CollectionManager.Common.Interfaces.Controls;
public interface IMainFormView : IForm
{
    /// <summary>Runs an action on the UI thread of the main window (used to refresh views after background work).</summary>
    void InvokeOnUIThread(Action action);

    event GuiHelpers.LoadFileArgs OnLoadFile;
    ICombinedListingView CombinedListingView { get; }
    ICombinedBeatmapPreviewView CombinedBeatmapPreviewView { get; }
    IMainSidePanelView SidePanelView { get; }
    ICollectionTextView CollectionTextView { get; }
    IInfoTextView InfoTextView { get; }
    IScoresListingView ScoresListingView { get; }
}