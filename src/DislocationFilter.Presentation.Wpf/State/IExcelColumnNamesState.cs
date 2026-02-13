namespace DislocationFilter.Presentation.Wpf.State;

public interface IExcelColumnNamesState
{
    IReadOnlyList<string> ColumnNames { get; }

    void Set(IReadOnlyList<string> columnNames);
}
