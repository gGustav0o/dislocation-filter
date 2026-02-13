using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;

namespace DislocationFilter.Application.UseCases.Excel.GetColumnNames;

public sealed record GetExcelColumnNamesQuery(string FilePath) : IQuery<Result<string, IReadOnlyList<string>>>;
