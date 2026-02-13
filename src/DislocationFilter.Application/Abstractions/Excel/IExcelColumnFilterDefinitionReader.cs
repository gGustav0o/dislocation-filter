using DislocationFilter.Application.Models;

namespace DislocationFilter.Application.Abstractions.Excel;

public interface IExcelColumnFilterDefinitionReader
{
    Task<ExcelColumnFilterDefinition?> ReadFilterDefinitionAsync(
        string filePath,
        string columnName,
        CancellationToken cancellationToken);
}
