using DislocationFilter.Application.Abstractions.Excel;
using DislocationFilter.Application.Abstractions.Time;
using DislocationFilter.Infrastructure.Excel;
using DislocationFilter.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace DislocationFilter.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IExcelColumnNameReader, ClosedXmlExcelColumnNameReader>();
        services.AddSingleton<IExcelColumnFilterDefinitionReader, ClosedXmlExcelColumnFilterDefinitionReader>();
        services.AddSingleton<IExcelFilteredWorkbookExporter, ClosedXmlFilteredWorkbookExporter>();
        return services;
    }
}
