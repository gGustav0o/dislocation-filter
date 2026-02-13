using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;
using DislocationFilter.Application.Models;

namespace DislocationFilter.Application.UseCases.Excel.ExportFilteredWorkbook;

public sealed record ExportFilteredWorkbookCommand(
    string SourceFilePath,
    string OutputFilePath,
    IReadOnlyList<ExcelFilterCriterion> Filters)
    : ICommand<Result<string, ExportFilteredWorkbookResult>>;
