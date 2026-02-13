using System.Windows;
using DislocationFilter.Presentation.Wpf.ViewModels;

namespace DislocationFilter.Presentation.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
