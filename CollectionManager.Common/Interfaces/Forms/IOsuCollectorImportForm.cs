namespace CollectionManager.Common.Interfaces.Forms;

using System;

/// <summary>Values for the "after import" behavior setting.</summary>
public static class OsuCollectorImportBehaviors
{
    public const string Ask = "Ask";
    public const string DownloadDirectly = "Yes";
    public const string DoNothing = "No";
}

/// <summary>
/// View for the "Import collection from osu!collector" window.
/// The presenter owns all data; the view only collects input and renders state.
/// </summary>
public interface IOsuCollectorImportForm : IForm
{
    /// <summary>Collection link or numeric ID entered by the user.</summary>
    string CollectionLink { get; }

    /// <summary>Selected behavior after a successful import: "Ask", "Yes" (download directly) or "No".</summary>
    string ImportAfterDownloadBehavior { get; set; }

    /// <summary>Shows the import result summary (name, uploader, beatmap count, description).</summary>
    void SetResultSummary(string text);

    /// <summary>Enables/disables the import button while a request is in flight.</summary>
    void SetImportingState(bool importing);

    event EventHandler ImportClicked;
    event EventHandler BehaviorChanged;
}