namespace DislocationFilter.Presentation.Wpf.Dialogs;

public interface ISaveExcelFileDialogService
{
    string? RequestOutputFilePath(string sourceFilePath);
}
