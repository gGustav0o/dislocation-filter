using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;
using DislocationFilter.Application.Models;

namespace DislocationFilter.Application.UseCases.Excel.GetColumnFilterDefinition;

public sealed record GetExcelColumnFilterDefinitionQuery(string FilePath, string ColumnName)
    : IQuery<Result<string, ExcelColumnFilterDefinition>>;
