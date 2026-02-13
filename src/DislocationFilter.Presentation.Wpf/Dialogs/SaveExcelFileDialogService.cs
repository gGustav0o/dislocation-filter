using System.IO;
using Microsoft.Win32;

namespace DislocationFilter.Presentation.Wpf.Dialogs;

public sealed class SaveExcelFileDialogService : ISaveExcelFileDialogService
{
    public string? RequestOutputFilePath(string sourceFilePath)
    {
        var sourceName = Path.GetFileNameWithoutExtension(sourceFilePath);
        var defaultName = string.IsNullOrWhiteSpace(sourceName)
            ? $"filtered_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            : $"{sourceName}_filtered_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        var dialog = new SaveFileDialog
        {
            Title = "Save filtered workbook",
            Filter = "Excel files (*.xlsx)|*.xlsx",
            FileName = defaultName,
            AddExtension = true,
            DefaultExt = ".xlsx",
            OverwritePrompt = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
