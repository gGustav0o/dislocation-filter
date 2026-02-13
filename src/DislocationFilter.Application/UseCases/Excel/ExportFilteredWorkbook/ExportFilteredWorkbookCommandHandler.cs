using DislocationFilter.Application.Abstractions.Excel;
using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;
using DislocationFilter.Application.Models;

namespace DislocationFilter.Application.UseCases.Excel.ExportFilteredWorkbook;

public sealed class ExportFilteredWorkbookCommandHandler
    : ICommandHandler<ExportFilteredWorkbookCommand, Result<string, ExportFilteredWorkbookResult>>
{
    private readonly IExcelFilteredWorkbookExporter _exporter;

    public ExportFilteredWorkbookCommandHandler(IExcelFilteredWorkbookExporter exporter)
    {
        _exporter = exporter;
    }

    public async Task<Result<string, ExportFilteredWorkbookResult>> Handle(
        ExportFilteredWorkbookCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SourceFilePath) || !File.Exists(command.SourceFilePath))
        {
            return Result<string, ExportFilteredWorkbookResult>.Failure("Source Excel file is missing.");
        }

        if (string.IsNullOrWhiteSpace(command.OutputFilePath))
        {
            return Result<string, ExportFilteredWorkbookResult>.Failure("Output file path is empty.");
        }

        if (command.Filters.Count == 0)
        {
            return Result<string, ExportFilteredWorkbookResult>.Failure("Filter list is empty.");
        }

        var result = await _exporter.ExportAsync(
            command.SourceFilePath,
            command.OutputFilePath,
            command.Filters,
            cancellationToken);

        return Result<string, ExportFilteredWorkbookResult>.Success(result);
    }
}
