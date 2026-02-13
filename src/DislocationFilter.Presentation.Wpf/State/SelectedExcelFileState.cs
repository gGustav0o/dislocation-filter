namespace DislocationFilter.Presentation.Wpf.State;

public sealed class SelectedExcelFileState : ISelectedExcelFileState
{
    public bool HasSelection => !string.IsNullOrWhiteSpace(FilePath);

    public string? FilePath { get; private set; }

    public void Set(string filePath)
    {
        FilePath = filePath;
    }
}
