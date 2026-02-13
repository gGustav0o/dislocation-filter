using System.Windows.Input;

namespace DislocationFilter.Presentation.Wpf.Commands;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync(object? parameter = null);
}
