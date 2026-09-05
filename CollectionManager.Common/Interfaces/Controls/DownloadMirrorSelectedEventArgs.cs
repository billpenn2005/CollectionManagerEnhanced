namespace CollectionManager.Common.Interfaces.Controls;

using System;

/// <summary>Raised when the user picks a specific download mirror from the context menu.</summary>
public class DownloadMirrorSelectedEventArgs : EventArgs
{
    public DownloadMirrorSelectedEventArgs(string mirrorName)
    {
        MirrorName = mirrorName;
    }

    public string MirrorName { get; }
}