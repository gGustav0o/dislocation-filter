using DislocationFilter.Application.DependencyInjection;
using DislocationFilter.Infrastructure.DependencyInjection;
using DislocationFilter.Presentation.Wpf.Dialogs;
using DislocationFilter.Presentation.Wpf.Navigation;
using DislocationFilter.Presentation.Wpf.State;
using DislocationFilter.Presentation.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DislocationFilter.Presentation.Wpf.Composition;

public static class ServiceProviderFactory
{
    public static ServiceProvider Create()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        services.AddInfrastructure();

        services.AddSingleton<INavigationService, NoopNavigationService>();
        services.AddSingleton<IExcelFileDialogService, ExcelFileDialogService>();
        services.AddSingleton<ISaveExcelFileDialogService, SaveExcelFileDialogService>();
        services.AddSingleton<ISelectedExcelFileState, SelectedExcelFileState>();
        services.AddSingleton<IExcelColumnNamesState, ExcelColumnNamesState>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
