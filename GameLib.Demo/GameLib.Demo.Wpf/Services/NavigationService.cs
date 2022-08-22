using GameLib.Demo.Wpf.Store;
using GameLib.Demo.Wpf.ViewModels;
using System;

namespace GameLib.Demo.Wpf.Services;

public class NavigationService<TViewModel> : INavigationService<TViewModel>
    where TViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;
    private readonly Func<TViewModel> _createViewModel;

    public NavigationService(NavigationStore navigationStore, Func<TViewModel> createViewModel)
    {
        _navigationStore = navigationStore;
        _createViewModel = createViewModel;
    }

    public void Navigate()
    {
        _navigationStore.CurrentViewModel = _createViewModel();
    }
}
