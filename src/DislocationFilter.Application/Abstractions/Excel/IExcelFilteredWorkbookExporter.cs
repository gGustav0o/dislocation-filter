using DislocationFilter.Application.Models;

namespace DislocationFilter.Application.Abstractions.Excel;

public interface IExcelFilteredWorkbookExporter
{
    Task<ExportFilteredWorkbookResult> ExportAsync(
        string sourceFilePath,
        string outputFilePath,
        IReadOnlyList<ExcelFilterCriterion> filters,
        CancellationToken cancellationToken);
}
