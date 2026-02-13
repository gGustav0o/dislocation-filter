namespace DislocationFilter.Application.Abstractions.Excel;

public interface IExcelColumnNameReader
{
    Task<IReadOnlyList<string>> ReadColumnNamesAsync(string filePath, CancellationToken cancellationToken);
}
