namespace CollectionManager.App.Shared.Misc.SidePanelActions;

using CollectionManager.App.Shared.Presenters.Forms;
using CollectionManager.Common;
using CollectionManager.Common.Interfaces;
using CollectionManager.Common.Interfaces.Forms;

/// <summary>Opens the in-app "Download sources" settings window (mirror management).</summary>
public sealed class DownloadSourcesHandler : IMainSidePanelActionHandler
{
    private readonly IUserDialogs _userDialogs;
    private IDownloadSourcesForm _downloadSourcesForm;

    public MainSidePanelActions Action { get; } = MainSidePanelActions.DownloadSources;

    public DownloadSourcesHandler(IUserDialogs userDialogs)
    {
        _userDialogs = userDialogs;
    }

    public Task HandleAsync(object sender, object data)
    {
        if (_downloadSourcesForm is not null && !_downloadSourcesForm.IsDisposed)
        {
            _downloadSourcesForm.Show();
            return Task.CompletedTask;
        }

        _downloadSourcesForm = Initalizer.GuiComponentsProvider.GetClassImplementing<IDownloadSourcesForm>();
        _ = new DownloadSourcesPresenter(_downloadSourcesForm, _userDialogs);

        return Task.CompletedTask;
    }
}