using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;
using DislocationFilter.Application.Models;
using DislocationFilter.Application.UseCases.Excel.ExportFilteredWorkbook;
using DislocationFilter.Application.UseCases.Excel.GetColumnFilterDefinition;
using DislocationFilter.Application.UseCases.Excel.GetColumnNames;
using DislocationFilter.Application.UseCases.MainWindow.ReloadMainTitle;
using Microsoft.Extensions.DependencyInjection;

namespace DislocationFilter.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<
            ICommandHandler<ReloadMainTitleCommand, Result<string, string>>,
            ReloadMainTitleCommandHandler>();

        services.AddSingleton<
            ICommandHandler<ExportFilteredWorkbookCommand, Result<string, ExportFilteredWorkbookResult>>,
            ExportFilteredWorkbookCommandHandler>();

        services.AddSingleton<
            IQueryHandler<GetExcelColumnNamesQuery, Result<string, IReadOnlyList<string>>>,
            GetExcelColumnNamesQueryHandler>();

        services.AddSingleton<
            IQueryHandler<GetExcelColumnFilterDefinitionQuery, Result<string, ExcelColumnFilterDefinition>>,
            GetExcelColumnFilterDefinitionQueryHandler>();

        return services;
    }
}
