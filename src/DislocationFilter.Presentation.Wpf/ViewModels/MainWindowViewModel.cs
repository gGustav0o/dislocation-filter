using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;
using DislocationFilter.Application.Models;
using DislocationFilter.Application.UseCases.Excel.ExportFilteredWorkbook;
using DislocationFilter.Application.UseCases.Excel.GetColumnFilterDefinition;
using DislocationFilter.Application.UseCases.MainWindow.ReloadMainTitle;
using DislocationFilter.Presentation.Wpf.Commands;
using DislocationFilter.Presentation.Wpf.Dialogs;
using DislocationFilter.Presentation.Wpf.Navigation;
using DislocationFilter.Presentation.Wpf.State;

namespace DislocationFilter.Presentation.Wpf.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private const string DefaultTitle = "Dislocation Filter";

    private readonly INavigationService _navigationService;
    private readonly ISaveExcelFileDialogService _saveExcelFileDialogService;
    private readonly ICommandHandler<ReloadMainTitleCommand, Result<string, string>> _reloadMainTitleHandler;
    private readonly ICommandHandler<ExportFilteredWorkbookCommand, Result<string, ExportFilteredWorkbookResult>> _exportFilteredWorkbookHandler;
    private readonly IQueryHandler<GetExcelColumnFilterDefinitionQuery, Result<string, ExcelColumnFilterDefinition>> _filterDefinitionHandler;
    private readonly ISelectedExcelFileState _selectedExcelFileState;
    private readonly IExcelColumnNamesState _excelColumnNamesState;

    private string _title;
    private string _selectedExcelFilePath;
    private string? _selectedColumnName;
    private ExcelColumnValueType _selectedColumnValueType;
    private string _selectedColumnValueTypeText;
    private IReadOnlyList<string> _availableOperations;
    private string? _selectedOperation;
    private string _filterError;
    private string _exportStatus;

    public MainWindowViewModel(
        INavigationService navigationService,
        ISaveExcelFileDialogService saveExcelFileDialogService,
        ICommandHandler<ReloadMainTitleCommand, Result<string, string>> reloadMainTitleHandler,
        ICommandHandler<ExportFilteredWorkbookCommand, Result<string, ExportFilteredWorkbookResult>> exportFilteredWorkbookHandler,
        IQueryHandler<GetExcelColumnFilterDefinitionQuery, Result<string, ExcelColumnFilterDefinition>> filterDefinitionHandler,
        ISelectedExcelFileState selectedExcelFileState,
        IExcelColumnNamesState excelColumnNamesState)
    {
        _navigationService = navigationService;
        _saveExcelFileDialogService = saveExcelFileDialogService;
        _reloadMainTitleHandler = reloadMainTitleHandler;
        _exportFilteredWorkbookHandler = exportFilteredWorkbookHandler;
        _filterDefinitionHandler = filterDefinitionHandler;
        _selectedExcelFileState = selectedExcelFileState;
        _excelColumnNamesState = excelColumnNamesState;

        _selectedExcelFilePath = _selectedExcelFileState.FilePath ?? string.Empty;
        _title = BuildTitle(DefaultTitle, _selectedExcelFilePath);

        _selectedColumnValueType = ExcelColumnValueType.Text;
        _selectedColumnValueTypeText = "Text";
        _availableOperations = Array.Empty<string>();
        _filterError = string.Empty;
        _exportStatus = string.Empty;

        ConfiguredFilters = new ObservableCollection<ConfiguredFilterItem>();
        ConfiguredFilters.CollectionChanged += (_, _) => CommandManager.InvalidateRequerySuggested();

        GoBackCommand = new RelayCommand(_navigationService.GoBack, () => _navigationService.CanGoBack);
        ResetTitleCommand = new RelayCommand(ResetTitle);
        ReloadTitleCommand = new AsyncCommand(_ => ReloadTitleAsync());
        AddFilterCommand = new RelayCommand(_ => AddCurrentFilter(), _ => CanAddCurrentFilter());
        RemoveFilterCommand = new RelayCommand(RemoveFilter);
        ExportFilteredWorkbookCommand = new AsyncCommand(_ => ExportFilteredWorkbookAsync(), () => CanExportFilteredWorkbook());

        if (ColumnNames.Count > 0)
        {
            SelectedColumnName = ColumnNames[0];
        }
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public string SelectedExcelFilePath
    {
        get => _selectedExcelFilePath;
        private set => SetProperty(ref _selectedExcelFilePath, value);
    }

    public IReadOnlyList<string> ColumnNames => _excelColumnNamesState.ColumnNames;

    public ObservableCollection<ConfiguredFilterItem> ConfiguredFilters { get; }

    public string ExportStatus
    {
        get => _exportStatus;
        private set => SetProperty(ref _exportStatus, value);
    }

    public string? SelectedColumnName
    {
        get => _selectedColumnName;
        set
        {
            if (!SetProperty(ref _selectedColumnName, value))
            {
                return;
            }

            _ = LoadFilterDefinitionAsync();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string SelectedColumnValueTypeText
    {
        get => _selectedColumnValueTypeText;
        private set => SetProperty(ref _selectedColumnValueTypeText, value);
    }

    public IReadOnlyList<string> AvailableOperations
    {
        get => _availableOperations;
        private set => SetProperty(ref _availableOperations, value);
    }

    public string? SelectedOperation
    {
        get => _selectedOperation;
        set
        {
            if (!SetProperty(ref _selectedOperation, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsTextEqualsVisible));
            OnPropertyChanged(nameof(IsTextContainsVisible));
            OnPropertyChanged(nameof(IsTextListVisible));
            OnPropertyChanged(nameof(IsNumberEqualsVisible));
            OnPropertyChanged(nameof(IsNumberRangeVisible));
            OnPropertyChanged(nameof(IsNumberListVisible));
            OnPropertyChanged(nameof(IsDateEqualsVisible));
            OnPropertyChanged(nameof(IsDateRangeVisible));
            OnPropertyChanged(nameof(IsDateListVisible));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string FilterError
    {
        get => _filterError;
        private set => SetProperty(ref _filterError, value);
    }

    public string TextEqualsValue { get; set; } = string.Empty;

    public string TextContainsValue { get; set; } = string.Empty;

    public string TextListRaw { get; set; } = string.Empty;

    public string NumberEqualsValue { get; set; } = string.Empty;

    public string NumberRangeFrom { get; set; } = string.Empty;

    public string NumberRangeTo { get; set; } = string.Empty;

    public string NumberListRaw { get; set; } = string.Empty;

    public DateTime? DateEqualsValue { get; set; }

    public DateTime? DateRangeFrom { get; set; }

    public DateTime? DateRangeTo { get; set; }

    public string DateListRaw { get; set; } = string.Empty;

    public bool IsTextEqualsVisible => _selectedColumnValueType == ExcelColumnValueType.Text && SelectedOperation == "Equals";

    public bool IsTextContainsVisible => _selectedColumnValueType == ExcelColumnValueType.Text && SelectedOperation == "Contains";

    public bool IsTextListVisible => _selectedColumnValueType == ExcelColumnValueType.Text && SelectedOperation == "In list";

    public bool IsNumberEqualsVisible => _selectedColumnValueType == ExcelColumnValueType.Number && SelectedOperation == "Equals";

    public bool IsNumberRangeVisible => _selectedColumnValueType == ExcelColumnValueType.Number && SelectedOperation == "In range";

    public bool IsNumberListVisible => _selectedColumnValueType == ExcelColumnValueType.Number && SelectedOperation == "In list";

    public bool IsDateEqualsVisible => _selectedColumnValueType == ExcelColumnValueType.Date && SelectedOperation == "Equals";

    public bool IsDateRangeVisible => _selectedColumnValueType == ExcelColumnValueType.Date && SelectedOperation == "In range";

    public bool IsDateListVisible => _selectedColumnValueType == ExcelColumnValueType.Date && SelectedOperation == "In list";

    public ICommand ResetTitleCommand { get; }

    public ICommand GoBackCommand { get; }

    public IAsyncCommand ReloadTitleCommand { get; }

    public ICommand AddFilterCommand { get; }

    public ICommand RemoveFilterCommand { get; }

    public IAsyncCommand ExportFilteredWorkbookCommand { get; }

    private void ResetTitle()
    {
        Title = BuildTitle(DefaultTitle, SelectedExcelFilePath);
    }

    private async Task ReloadTitleAsync()
    {
        var result = await _reloadMainTitleHandler.Handle(
            new ReloadMainTitleCommand(DefaultTitle),
            CancellationToken.None);

        Title = result.Match(
            onSuccess: updatedTitle => BuildTitle(updatedTitle, SelectedExcelFilePath),
            onFailure: error => $"Error: {error}");
    }

    private async Task LoadFilterDefinitionAsync()
    {
        FilterError = string.Empty;

        if (string.IsNullOrWhiteSpace(SelectedColumnName) || string.IsNullOrWhiteSpace(SelectedExcelFilePath))
        {
            AvailableOperations = Array.Empty<string>();
            SelectedOperation = null;
            return;
        }

        Result<string, ExcelColumnFilterDefinition> result;
        try
        {
            result = await _filterDefinitionHandler.Handle(
                new GetExcelColumnFilterDefinitionQuery(SelectedExcelFilePath, SelectedColumnName),
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            FilterError = ex.Message;
            AvailableOperations = Array.Empty<string>();
            SelectedOperation = null;
            return;
        }

        if (result.IsFailure)
        {
            FilterError = result.Error ?? "Unknown error.";
            AvailableOperations = Array.Empty<string>();
            SelectedOperation = null;
            return;
        }

        var definition = result.Value!;
        _selectedColumnValueType = definition.ValueType;
        SelectedColumnValueTypeText = definition.ValueType.ToString();
        AvailableOperations = definition.Operations;
        SelectedOperation = AvailableOperations.FirstOrDefault();
    }

    private bool CanAddCurrentFilter()
    {
        return !string.IsNullOrWhiteSpace(SelectedColumnName)
            && !string.IsNullOrWhiteSpace(SelectedOperation)
            && string.IsNullOrWhiteSpace(FilterError);
    }

    private void AddCurrentFilter()
    {
        if (!TryBuildFilterValue(out var value, out var error))
        {
            FilterError = error;
            return;
        }

        var columnName = SelectedColumnName!;
        var operation = SelectedOperation!;
        var valueTypeText = _selectedColumnValueType.ToString();
        var display = $"{columnName} | {valueTypeText} | {operation} | {value}";

        ConfiguredFilters.Add(new ConfiguredFilterItem(columnName, _selectedColumnValueType, operation, value, display));
        FilterError = string.Empty;
        ExportStatus = string.Empty;
    }

    private void RemoveFilter(object? parameter)
    {
        if (parameter is not ConfiguredFilterItem item)
        {
            return;
        }

        ConfiguredFilters.Remove(item);
    }

    private bool CanExportFilteredWorkbook()
    {
        return !string.IsNullOrWhiteSpace(SelectedExcelFilePath)
            && ConfiguredFilters.Count > 0;
    }

    private async Task ExportFilteredWorkbookAsync()
    {
        FilterError = string.Empty;
        ExportStatus = string.Empty;

        var outputPath = _saveExcelFileDialogService.RequestOutputFilePath(SelectedExcelFilePath);
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return;
        }

        var criteria = ConfiguredFilters
            .Select(x => new ExcelFilterCriterion(x.ColumnName, x.ValueType, x.Operation, x.Value))
            .ToArray();

        var result = await _exportFilteredWorkbookHandler.Handle(
            new ExportFilteredWorkbookCommand(SelectedExcelFilePath, outputPath, criteria),
            CancellationToken.None);

        if (result.IsFailure)
        {
            FilterError = result.Error ?? "Export failed.";
            return;
        }

        var exportResult = result.Value!;
        ExportStatus = $"Saved: {exportResult.OutputFilePath}. Rows exported: {exportResult.ExportedRowCount}.";
    }

    private bool TryBuildFilterValue(out string value, out string error)
    {
        value = string.Empty;
        error = string.Empty;

        if (SelectedOperation is null)
        {
            error = "Select operation.";
            return false;
        }

        if (_selectedColumnValueType == ExcelColumnValueType.Text)
        {
            return TryBuildTextFilterValue(SelectedOperation, out value, out error);
        }

        if (_selectedColumnValueType == ExcelColumnValueType.Number)
        {
            return TryBuildNumberFilterValue(SelectedOperation, out value, out error);
        }

        return TryBuildDateFilterValue(SelectedOperation, out value, out error);
    }

    private bool TryBuildTextFilterValue(string operation, out string value, out string error)
    {
        value = string.Empty;
        error = string.Empty;

        if (operation == "Equals")
        {
            if (string.IsNullOrWhiteSpace(TextEqualsValue))
            {
                error = "Text value is empty.";
                return false;
            }

            value = TextEqualsValue.Trim();
            return true;
        }

        if (operation == "Contains")
        {
            if (string.IsNullOrWhiteSpace(TextContainsValue))
            {
                error = "Substring is empty.";
                return false;
            }

            value = TextContainsValue.Trim();
            return true;
        }

        var parts = ParseCsvList(TextListRaw);
        if (parts.Count == 0)
        {
            error = "Text list is empty.";
            return false;
        }

        value = string.Join(", ", parts);
        return true;
    }

    private bool TryBuildNumberFilterValue(string operation, out string value, out string error)
    {
        value = string.Empty;
        error = string.Empty;

        if (operation == "Equals")
        {
            if (!TryParseDouble(NumberEqualsValue, out var parsed))
            {
                error = "Invalid number.";
                return false;
            }

            value = parsed.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        if (operation == "In range")
        {
            if (!TryParseDouble(NumberRangeFrom, out var from) || !TryParseDouble(NumberRangeTo, out var to))
            {
                error = "Invalid number range.";
                return false;
            }

            if (from > to)
            {
                error = "Range start is greater than range end.";
                return false;
            }

            value = $"[{from.ToString(CultureInfo.InvariantCulture)}; {to.ToString(CultureInfo.InvariantCulture)}]";
            return true;
        }

        var parts = ParseCsvList(NumberListRaw);
        var parsedNumbers = new List<double>();
        foreach (var part in parts)
        {
            if (!TryParseDouble(part, out var number))
            {
                error = $"Invalid number in list: {part}";
                return false;
            }

            parsedNumbers.Add(number);
        }

        if (parsedNumbers.Count == 0)
        {
            error = "Number list is empty.";
            return false;
        }

        value = string.Join(", ", parsedNumbers.Select(n => n.ToString(CultureInfo.InvariantCulture)));
        return true;
    }

    private bool TryBuildDateFilterValue(string operation, out string value, out string error)
    {
        value = string.Empty;
        error = string.Empty;

        if (operation == "Equals")
        {
            if (DateEqualsValue is null)
            {
                error = "Date is not selected.";
                return false;
            }

            value = DateEqualsValue.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return true;
        }

        if (operation == "In range")
        {
            if (DateRangeFrom is null || DateRangeTo is null)
            {
                error = "Date range is incomplete.";
                return false;
            }

            if (DateRangeFrom.Value.Date > DateRangeTo.Value.Date)
            {
                error = "Date range start is greater than end.";
                return false;
            }

            value = $"[{DateRangeFrom.Value:yyyy-MM-dd}; {DateRangeTo.Value:yyyy-MM-dd}]";
            return true;
        }

        var parts = ParseCsvList(DateListRaw);
        var parsedDates = new List<DateTime>();
        foreach (var part in parts)
        {
            if (!DateTime.TryParse(part, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDate)
                && !DateTime.TryParse(part, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                error = $"Invalid date in list: {part}";
                return false;
            }

            parsedDates.Add(parsedDate.Date);
        }

        if (parsedDates.Count == 0)
        {
            error = "Date list is empty.";
            return false;
        }

        value = string.Join(", ", parsedDates.Select(d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        return true;
    }

    private static bool TryParseDouble(string raw, out double value)
    {
        return double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
            || double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static IReadOnlyList<string> ParseCsvList(string raw)
    {
        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

    private static string BuildTitle(string baseTitle, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return baseTitle;
        }

        return $"{baseTitle} ({Path.GetFileName(filePath)})";
    }
}
