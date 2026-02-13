namespace DislocationFilter.Application.Models;

public sealed record ExcelFilterCriterion(
    string ColumnName,
    ExcelColumnValueType ValueType,
    string Operation,
    string Value);
