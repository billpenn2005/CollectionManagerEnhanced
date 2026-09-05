namespace GuiComponents.Forms;

using CollectionManager.Common.Interfaces.Forms;
using CollectionManager.WinForms.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

public partial class DownloadSourcesForm : BaseForm, IDownloadSourcesForm
{
    public DownloadSourcesForm()
    {
        InitializeComponent();

        listBox_sources.SelectedIndexChanged += (_, _) => SelectedSourceChanged?.Invoke(this, EventArgs.Empty);
        listBox_mirrors.SelectedIndexChanged += (_, _) => MirrorSelectionChanged?.Invoke(this, EventArgs.Empty);
        button_addMirror.Click += (_, _) => AddMirrorRequested?.Invoke(this, EventArgs.Empty);
        button_removeMirror.Click += (_, _) => RemoveMirrorRequested?.Invoke(this, EventArgs.Empty);
        button_moveUp.Click += (_, _) => MoveMirrorUpRequested?.Invoke(this, EventArgs.Empty);
        button_moveDown.Click += (_, _) => MoveMirrorDownRequested?.Invoke(this, EventArgs.Empty);
        button_save.Click += (_, _) => SaveRequested?.Invoke(this, EventArgs.Empty);
        button_close.Click += (_, _) => Close();
    }

    public event EventHandler SelectedSourceChanged;
    public event EventHandler MirrorSelectionChanged;
    public event EventHandler AddMirrorRequested;
    public event EventHandler RemoveMirrorRequested;
    public event EventHandler MoveMirrorUpRequested;
    public event EventHandler MoveMirrorDownRequested;
    public event EventHandler SaveRequested;

    public int SelectedSourceIndex => listBox_sources.SelectedIndex;

    public int SelectedMirrorIndex
    {
        get => listBox_mirrors.SelectedIndex;
        set
        {
            if (value >= 0 && value < listBox_mirrors.Items.Count)
            {
                listBox_mirrors.SelectedIndex = value;
            }
        }
    }

    public string MirrorName => textBox_mirrorName.Text;
    public string MirrorUrl => textBox_mirrorUrl.Text;
    public string MirrorUrlNoVideo => textBox_mirrorUrlNoVideo.Text;
    public string MirrorReferer => textBox_mirrorReferer.Text;

    public void SetSources(IReadOnlyList<string> names)
    {
        listBox_sources.Items.Clear();
        listBox_sources.Items.AddRange(names.ToArray());
        if (listBox_sources.Items.Count > 0)
        {
            listBox_sources.SelectedIndex = 0;
        }
    }

    public void SetSourceInfo(string info) => textBox_sourceInfo.Text = info;

    public void SetMirrors(IReadOnlyList<DownloadSourceMirrorEdit> mirrors)
    {
        listBox_mirrors.Items.Clear();
        listBox_mirrors.Items.AddRange(mirrors.Select(m => m.Name).ToArray());
        if (listBox_mirrors.Items.Count > 0)
        {
            listBox_mirrors.SelectedIndex = 0;
        }
    }

    public void SetMirrorEdit(DownloadSourceMirrorEdit mirror)
    {
        textBox_mirrorName.Text = mirror.Name;
        textBox_mirrorUrl.Text = mirror.Url;
        textBox_mirrorUrlNoVideo.Text = mirror.UrlNoVideo;
        textBox_mirrorReferer.Text = mirror.Referer;
    }

    public void SetMirrorEditEnabled(bool enabled)
    {
        listBox_mirrors.Enabled = enabled;
        textBox_mirrorName.Enabled = enabled;
        textBox_mirrorUrl.Enabled = enabled;
        textBox_mirrorUrlNoVideo.Enabled = enabled;
        textBox_mirrorReferer.Enabled = enabled;
        button_addMirror.Enabled = enabled;
        button_removeMirror.Enabled = enabled;
        button_moveUp.Enabled = enabled;
        button_moveDown.Enabled = enabled;
    }

    public void SetSaveEnabled(bool enabled) => button_save.Enabled = enabled;
}