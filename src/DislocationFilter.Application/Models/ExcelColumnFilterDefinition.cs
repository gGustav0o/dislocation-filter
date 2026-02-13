namespace DislocationFilter.Application.Models;

public sealed record ExcelColumnFilterDefinition(
    string ColumnName,
    ExcelColumnValueType ValueType,
    IReadOnlyList<string> Operations);
