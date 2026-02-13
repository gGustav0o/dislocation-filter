using DislocationFilter.Application.Abstractions.Excel;
using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;

namespace DislocationFilter.Application.UseCases.Excel.GetColumnNames;

public sealed class GetExcelColumnNamesQueryHandler
    : IQueryHandler<GetExcelColumnNamesQuery, Result<string, IReadOnlyList<string>>>
{
    private readonly IExcelColumnNameReader _excelColumnNameReader;

    public GetExcelColumnNamesQueryHandler(IExcelColumnNameReader excelColumnNameReader)
    {
        _excelColumnNameReader = excelColumnNameReader;
    }

    public async Task<Result<string, IReadOnlyList<string>>> Handle(
        GetExcelColumnNamesQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.FilePath))
        {
            return Result<string, IReadOnlyList<string>>.Failure("Excel file path is empty.");
        }

        if (!File.Exists(query.FilePath))
        {
            return Result<string, IReadOnlyList<string>>.Failure("Excel file does not exist.");
        }

        var columns = await _excelColumnNameReader.ReadColumnNamesAsync(query.FilePath, cancellationToken);
        if (columns.Count == 0)
        {
            return Result<string, IReadOnlyList<string>>.Failure("No column names were found in the selected file.");
        }

        return Result<string, IReadOnlyList<string>>.Success(columns);
    }
}
