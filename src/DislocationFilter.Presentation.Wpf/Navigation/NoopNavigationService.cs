namespace DislocationFilter.Presentation.Wpf.Navigation;

public sealed class NoopNavigationService : INavigationService
{
    public bool CanGoBack => false;

    public void GoBack()
    {
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModels.ViewModelBase
    {
    }
}
