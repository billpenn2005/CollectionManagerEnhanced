namespace CollectionManager.App.Shared.Misc.SidePanelActions;

using CollectionManager.App.Shared.Models.Forms;
using CollectionManager.App.Shared.Presenters.Forms;
using CollectionManager.Common;
using CollectionManager.Common.Interfaces;
using CollectionManager.Common.Interfaces.Forms;
using System.Threading.Tasks;

public sealed class MergedOszExportHandler : IMainSidePanelActionHandler
{
    private readonly IUserDialogs _userDialogs;
    private IMergedOszExportForm _form;

    public MergedOszExportHandler(IUserDialogs userDialogs)
    {
        _userDialogs = userDialogs;
    }

    public MainSidePanelActions Action { get; } = MainSidePanelActions.MergeExportOsz;

    public Task HandleAsync(object sender, object data)
    {
        if (_form is null || _form.IsDisposed)
        {
            _form = Initalizer.GuiComponentsProvider.GetClassImplementing<IMergedOszExportForm>();
            _ = new MergedOszExportFormPresenter(_form, new MergedOszExportModel(), _userDialogs);
        }

        _form.Show();

        return Task.CompletedTask;
    }
}