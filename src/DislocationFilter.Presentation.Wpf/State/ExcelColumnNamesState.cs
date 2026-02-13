namespace DislocationFilter.Presentation.Wpf.State;

public sealed class ExcelColumnNamesState : IExcelColumnNamesState
{
    private IReadOnlyList<string> _columnNames = Array.Empty<string>();

    public IReadOnlyList<string> ColumnNames => _columnNames;

    public void Set(IReadOnlyList<string> columnNames)
    {
        _columnNames = columnNames;
    }
}
