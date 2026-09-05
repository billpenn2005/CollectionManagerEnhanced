namespace CollectionManager.App.Shared.Misc.SidePanelActions;

using CollectionManager.App.Shared.Presenters.Forms;
using CollectionManager.Common;
using CollectionManager.Common.Interfaces;
using CollectionManager.Common.Interfaces.Forms;

/// <summary>
/// Opens the "Import collection from osu!collector" window where a collection
/// can be imported from a link or numeric ID (https://osucollector.com).
/// </summary>
public sealed class OsuCollectorImportHandler : IMainSidePanelActionHandler
{
    private readonly IUserDialogs _userDialogs;
    private readonly ILoginFormView _loginForm;
    private IOsuCollectorImportForm _importForm;

    public MainSidePanelActions Action { get; } = MainSidePanelActions.OsuCollectorImport;

    public OsuCollectorImportHandler(IUserDialogs userDialogs, ILoginFormView loginForm)
    {
        _userDialogs = userDialogs;
        _loginForm = loginForm;
    }

    public Task HandleAsync(object sender, object data)
    {
        if (_importForm is not null && !_importForm.IsDisposed)
        {
            _importForm.Show();
            return Task.CompletedTask;
        }

        _importForm = Initalizer.GuiComponentsProvider.GetClassImplementing<IOsuCollectorImportForm>();
        _ = new OsuCollectorImportFormPresenter(_importForm, _userDialogs, _loginForm);

        return Task.CompletedTask;
    }
}