namespace GuiComponents.Forms;

using CollectionManager.Common.Interfaces.Forms;
using CollectionManager.WinForms.Forms;
using System;
using System.Windows.Forms;

public partial class OsuCollectorImportForm : BaseForm, IOsuCollectorImportForm
{
    private static readonly string[] BehaviorValues = [OsuCollectorImportBehaviors.Ask, OsuCollectorImportBehaviors.DownloadDirectly, OsuCollectorImportBehaviors.DoNothing];

    public OsuCollectorImportForm()
    {
        InitializeComponent();

        button_import.Click += (_, _) => ImportClicked?.Invoke(this, EventArgs.Empty);
        button_close.Click += (_, _) => Close();
        comboBox_behavior.SelectedIndexChanged += (_, _) => BehaviorChanged?.Invoke(this, EventArgs.Empty);
        comboBox_behavior.SelectedIndex = 0;
    }

    public event EventHandler ImportClicked;
    public event EventHandler BehaviorChanged;

    public string CollectionLink => textBox_link.Text;

    public string ImportAfterDownloadBehavior
    {
        get => comboBox_behavior.SelectedIndex >= 0 ? BehaviorValues[comboBox_behavior.SelectedIndex] : OsuCollectorImportBehaviors.Ask;
        set
        {
            int index = Array.IndexOf(BehaviorValues, value);
            comboBox_behavior.SelectedIndex = index >= 0 ? index : 0;
        }
    }

    public void SetResultSummary(string text) => textBox_summary.Text = text;

    public void SetImportingState(bool importing)
    {
        button_import.Enabled = !importing;
        textBox_link.Enabled = !importing;
    }
}