using ClosedXML.Excel;
using DislocationFilter.Application.Abstractions.Excel;

namespace DislocationFilter.Infrastructure.Excel;

public sealed class ClosedXmlExcelColumnNameReader : IExcelColumnNameReader
{
    public Task<IReadOnlyList<string>> ReadColumnNamesAsync(string filePath, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var headerRow = worksheet.FirstRowUsed();
        if (headerRow is null)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var names = headerRow.CellsUsed(XLCellsUsedOptions.AllContents)
            .Select(cell => cell.GetString().Trim())
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        return Task.FromResult<IReadOnlyList<string>>(names);
    }
}
