namespace DislocationFilter.Presentation.Wpf.Navigation;

public interface INavigationService
{
    bool CanGoBack { get; }

    void GoBack();

    void NavigateTo<TViewModel>() where TViewModel : ViewModels.ViewModelBase;
}
