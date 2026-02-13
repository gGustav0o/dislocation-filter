using DislocationFilter.Application.Abstractions.Excel;
using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;
using DislocationFilter.Application.Models;

namespace DislocationFilter.Application.UseCases.Excel.GetColumnFilterDefinition;

public sealed class GetExcelColumnFilterDefinitionQueryHandler
    : IQueryHandler<GetExcelColumnFilterDefinitionQuery, Result<string, ExcelColumnFilterDefinition>>
{
    private readonly IExcelColumnFilterDefinitionReader _reader;

    public GetExcelColumnFilterDefinitionQueryHandler(IExcelColumnFilterDefinitionReader reader)
    {
        _reader = reader;
    }

    public async Task<Result<string, ExcelColumnFilterDefinition>> Handle(
        GetExcelColumnFilterDefinitionQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.FilePath))
        {
            return Result<string, ExcelColumnFilterDefinition>.Failure("Excel file path is empty.");
        }

        if (string.IsNullOrWhiteSpace(query.ColumnName))
        {
            return Result<string, ExcelColumnFilterDefinition>.Failure("Column name is empty.");
        }

        if (!File.Exists(query.FilePath))
        {
            return Result<string, ExcelColumnFilterDefinition>.Failure("Excel file does not exist.");
        }

        var definition = await _reader.ReadFilterDefinitionAsync(query.FilePath, query.ColumnName, cancellationToken);
        if (definition is null)
        {
            return Result<string, ExcelColumnFilterDefinition>.Failure("Column was not found in the selected file.");
        }

        return Result<string, ExcelColumnFilterDefinition>.Success(definition);
    }
}
