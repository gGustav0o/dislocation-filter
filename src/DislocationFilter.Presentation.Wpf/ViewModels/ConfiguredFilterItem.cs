using DislocationFilter.Application.Models;

namespace DislocationFilter.Presentation.Wpf.ViewModels;

public sealed record ConfiguredFilterItem(
    string ColumnName,
    ExcelColumnValueType ValueType,
    string Operation,
    string Value,
    string DisplayText);
