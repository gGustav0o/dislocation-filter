namespace DislocationFilter.Application.Models;

public sealed record ExportFilteredWorkbookResult(
    string OutputFilePath,
    int ExportedRowCount);
