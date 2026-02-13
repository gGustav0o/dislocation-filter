namespace DislocationFilter.Presentation.Wpf.State;

public interface ISelectedExcelFileState
{
    bool HasSelection { get; }

    string? FilePath { get; }

    void Set(string filePath);
}
