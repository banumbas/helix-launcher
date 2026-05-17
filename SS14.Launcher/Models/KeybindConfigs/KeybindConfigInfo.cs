using System.IO;
using ReactiveUI;

namespace SS14.Launcher.Models.KeybindConfigs;

public sealed class KeybindConfigInfo : ReactiveObject
{
    private bool _selected;

    public KeybindConfigInfo(string filePath, bool selected)
    {
        FilePath = Path.GetFullPath(filePath);
        FileName = Path.GetFileName(FilePath);
        Name = Path.GetFileNameWithoutExtension(FilePath);
        _selected = selected;
    }

    public string FilePath { get; }
    public string FileName { get; }
    public string Name { get; }

    public bool Selected
    {
        get => _selected;
        set => this.RaiseAndSetIfChanged(ref _selected, value);
    }
}
