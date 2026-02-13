using ClosedXML.Excel;
using DislocationFilter.Application.Abstractions.Excel;
using DislocationFilter.Application.Models;

namespace DislocationFilter.Infrastructure.Excel;

public sealed class ClosedXmlExcelColumnFilterDefinitionReader : IExcelColumnFilterDefinitionReader
{
    private const int MaxSampleSize = 300;

    public Task<ExcelColumnFilterDefinition?> ReadFilterDefinitionAsync(
        string filePath,
        string columnName,
        CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
        {
            return Task.FromResult<ExcelColumnFilterDefinition?>(null);
        }

        var headerRow = worksheet.FirstRowUsed();
        if (headerRow is null)
        {
            return Task.FromResult<ExcelColumnFilterDefinition?>(null);
        }

        var headerCell = headerRow.CellsUsed(XLCellsUsedOptions.AllContents)
            .FirstOrDefault(cell => string.Equals(cell.GetString().Trim(), columnName, StringComparison.Ordinal));

        if (headerCell is null)
        {
            return Task.FromResult<ExcelColumnFilterDefinition?>(null);
        }

        var columnIndex = headerCell.Address.ColumnNumber;
        var valueType = DetectValueType(worksheet, headerRow.RowNumber(), columnIndex, cancellationToken);
        var operations = GetOperations(valueType);

        return Task.FromResult<ExcelColumnFilterDefinition?>(
            new ExcelColumnFilterDefinition(columnName, valueType, operations));
    }

    private static ExcelColumnValueType DetectValueType(
        IXLWorksheet worksheet,
        int headerRowNumber,
        int columnIndex,
        CancellationToken cancellationToken)
    {
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? headerRowNumber;

        var textCount = 0;
        var numberCount = 0;
        var dateCount = 0;
        var sampled = 0;

        for (var row = headerRowNumber + 1; row <= lastRow; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var cell = worksheet.Cell(row, columnIndex);
            if (cell.IsEmpty())
            {
                continue;
            }

            sampled++;

            if (cell.TryGetValue<DateTime>(out _))
            {
                dateCount++;
            }
            else if (cell.TryGetValue<double>(out _))
            {
                numberCount++;
            }
            else
            {
                textCount++;
            }

            if (sampled >= MaxSampleSize)
            {
                break;
            }
        }

        if (dateCount > 0 && numberCount == 0 && textCount == 0)
        {
            return ExcelColumnValueType.Date;
        }

        if (numberCount > 0 && dateCount == 0 && textCount == 0)
        {
            return ExcelColumnValueType.Number;
        }

        return ExcelColumnValueType.Text;
    }

    private static IReadOnlyList<string> GetOperations(ExcelColumnValueType valueType)
    {
        return valueType switch
        {
            ExcelColumnValueType.Number => new[] { "Equals", "In range", "In list" },
            ExcelColumnValueType.Date => new[] { "Equals", "In range", "In list" },
            _ => new[] { "Equals", "Contains", "In list" }
        };
    }
}
