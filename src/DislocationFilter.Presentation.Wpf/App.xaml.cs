using System.Windows;
using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;
using DislocationFilter.Application.UseCases.Excel.GetColumnNames;
using DislocationFilter.Presentation.Wpf.Composition;
using DislocationFilter.Presentation.Wpf.Dialogs;
using DislocationFilter.Presentation.Wpf.State;
using Microsoft.Extensions.DependencyInjection;

namespace DislocationFilter.Presentation.Wpf;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _serviceProvider = ServiceProviderFactory.Create();

        var fileDialogService = _serviceProvider.GetRequiredService<IExcelFileDialogService>();
        var selectedExcelFileState = _serviceProvider.GetRequiredService<ISelectedExcelFileState>();
        var excelColumnNamesState = _serviceProvider.GetRequiredService<IExcelColumnNamesState>();
        var getColumnsHandler = _serviceProvider.GetRequiredService<
            IQueryHandler<GetExcelColumnNamesQuery, Result<string, IReadOnlyList<string>>>>();

        while (true)
        {
            var selectedFilePath = fileDialogService.RequestExcelFilePath();
            if (!string.IsNullOrWhiteSpace(selectedFilePath))
            {
                selectedExcelFileState.Set(selectedFilePath);

                var result = await getColumnsHandler.Handle(
                    new GetExcelColumnNamesQuery(selectedFilePath),
                    CancellationToken.None);

                if (result.IsSuccess)
                {
                    excelColumnNamesState.Set(result.Value!);
                    break;
                }

                MessageBox.Show(
                    $"Failed to read column names: {result.Error}",
                    "Excel parsing error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            var exitRequested = MessageBox.Show(
                "Excel file is required to start the application. Exit now?",
                "Excel file required",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;

            if (exitRequested)
            {
                Shutdown();
                return;
            }
        }

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
