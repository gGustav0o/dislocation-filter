using Microsoft.Win32;

namespace DislocationFilter.Presentation.Wpf.Dialogs;

public sealed class ExcelFileDialogService : IExcelFileDialogService
{
    public string? RequestExcelFilePath()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Excel file",
            Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls",
            CheckFileExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
