using System.Globalization;
using ClosedXML.Excel;
using DislocationFilter.Application.Abstractions.Excel;
using DislocationFilter.Application.Models;

namespace DislocationFilter.Infrastructure.Excel;

public sealed class ClosedXmlFilteredWorkbookExporter : IExcelFilteredWorkbookExporter
{
    public Task<ExportFilteredWorkbookResult> ExportAsync(
        string sourceFilePath,
        string outputFilePath,
        IReadOnlyList<ExcelFilterCriterion> filters,
        CancellationToken cancellationToken)
    {
        using var sourceWorkbook = new XLWorkbook(sourceFilePath);
        var sourceSheet = sourceWorkbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("Source workbook does not contain worksheets.");

        var headerRow = sourceSheet.FirstRowUsed()
            ?? throw new InvalidOperationException("Header row was not found in source worksheet.");

        var headerRowNumber = headerRow.RowNumber();
        var headerCells = headerRow.CellsUsed(XLCellsUsedOptions.AllContents).ToArray();
        if (headerCells.Length == 0)
        {
            throw new InvalidOperationException("Header row is empty.");
        }

        var lastColumnNumber = headerCells.Max(c => c.Address.ColumnNumber);
        var columnsByName = headerCells
            .Select(c => new { Name = c.GetString().Trim(), Index = c.Address.ColumnNumber })
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Index, StringComparer.Ordinal);

        foreach (var filter in filters)
        {
            if (!columnsByName.ContainsKey(filter.ColumnName))
            {
                throw new InvalidOperationException($"Column not found: {filter.ColumnName}");
            }
        }

        using var outputWorkbook = new XLWorkbook();
        var outputSheet = outputWorkbook.AddWorksheet(sourceSheet.Name);

        CopyRow(sourceSheet, outputSheet, headerRowNumber, 1, lastColumnNumber);

        var lastRow = sourceSheet.LastRowUsed()?.RowNumber() ?? headerRowNumber;
        var targetRow = 2;
        var exportedCount = 0;

        for (var row = headerRowNumber + 1; row <= lastRow; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!MatchesAllFilters(sourceSheet, row, filters, columnsByName))
            {
                continue;
            }

            CopyRow(sourceSheet, outputSheet, row, targetRow, lastColumnNumber);
            targetRow++;
            exportedCount++;
        }

        var outputDirectory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        outputWorkbook.SaveAs(outputFilePath);

        return Task.FromResult(new ExportFilteredWorkbookResult(outputFilePath, exportedCount));
    }

    private static bool MatchesAllFilters(
        IXLWorksheet worksheet,
        int row,
        IReadOnlyList<ExcelFilterCriterion> filters,
        IReadOnlyDictionary<string, int> columnsByName)
    {
        foreach (var filter in filters)
        {
            var columnIndex = columnsByName[filter.ColumnName];
            var cell = worksheet.Cell(row, columnIndex);

            if (!MatchesFilter(cell, filter))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesFilter(IXLCell cell, ExcelFilterCriterion filter)
    {
        return filter.ValueType switch
        {
            ExcelColumnValueType.Number => MatchesNumber(cell, filter.Operation, filter.Value),
            ExcelColumnValueType.Date => MatchesDate(cell, filter.Operation, filter.Value),
            _ => MatchesText(cell, filter.Operation, filter.Value)
        };
    }

    private static bool MatchesText(IXLCell cell, string operation, string expected)
    {
        var actual = cell.GetString().Trim();

        if (operation == "Equals")
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }

        if (operation == "Contains")
        {
            return actual.Contains(expected, StringComparison.OrdinalIgnoreCase);
        }

        var allowed = SplitCsv(expected);
        return allowed.Any(x => string.Equals(x, actual, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesNumber(IXLCell cell, string operation, string expected)
    {
        if (!TryParseNumberFromCell(cell, out var actual))
        {
            return false;
        }

        if (operation == "Equals")
        {
            return TryParseDouble(expected, out var target) && Math.Abs(actual - target) < 0.0000001;
        }

        if (operation == "In range")
        {
            return TryParseNumberRange(expected, out var from, out var to) && actual >= from && actual <= to;
        }

        var values = SplitCsv(expected);
        foreach (var value in values)
        {
            if (TryParseDouble(value, out var parsed) && Math.Abs(actual - parsed) < 0.0000001)
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesDate(IXLCell cell, string operation, string expected)
    {
        if (!TryParseDateFromCell(cell, out var actual))
        {
            return false;
        }

        actual = actual.Date;

        if (operation == "Equals")
        {
            return TryParseDate(expected, out var target) && actual == target.Date;
        }

        if (operation == "In range")
        {
            return TryParseDateRange(expected, out var from, out var to)
                && actual >= from.Date
                && actual <= to.Date;
        }

        var values = SplitCsv(expected);
        foreach (var value in values)
        {
            if (TryParseDate(value, out var parsed) && actual == parsed.Date)
            {
                return true;
            }
        }

        return false;
    }

    private static void CopyRow(IXLWorksheet source, IXLWorksheet target, int sourceRow, int targetRow, int lastColumn)
    {
        for (var col = 1; col <= lastColumn; col++)
        {
            var sourceCell = source.Cell(sourceRow, col);
            var targetCell = target.Cell(targetRow, col);
            targetCell.Value = sourceCell.Value;
            targetCell.Style = sourceCell.Style;
        }
    }

    private static bool TryParseNumberFromCell(IXLCell cell, out double value)
    {
        if (cell.TryGetValue<double>(out value))
        {
            return true;
        }

        return TryParseDouble(cell.GetString(), out value);
    }

    private static bool TryParseDateFromCell(IXLCell cell, out DateTime value)
    {
        if (cell.TryGetValue<DateTime>(out value))
        {
            return true;
        }

        return TryParseDate(cell.GetString(), out value);
    }

    private static bool TryParseDouble(string raw, out double value)
    {
        return double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
            || double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseDate(string raw, out DateTime value)
    {
        return DateTime.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out value)
            || DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    private static bool TryParseNumberRange(string raw, out double from, out double to)
    {
        from = 0;
        to = 0;

        var normalized = raw.Trim().TrimStart('[').TrimEnd(']');
        var parts = normalized.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!TryParseDouble(parts[0], out from) || !TryParseDouble(parts[1], out to))
        {
            return false;
        }

        return from <= to;
    }

    private static bool TryParseDateRange(string raw, out DateTime from, out DateTime to)
    {
        from = default;
        to = default;

        var normalized = raw.Trim().TrimStart('[').TrimEnd(']');
        var parts = normalized.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!TryParseDate(parts[0], out from) || !TryParseDate(parts[1], out to))
        {
            return false;
        }

        return from.Date <= to.Date;
    }

    private static IReadOnlyList<string> SplitCsv(string raw)
    {
        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }
}
