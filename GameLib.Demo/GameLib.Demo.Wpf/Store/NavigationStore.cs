using GameLib.Demo.Wpf.ViewModels;
using System;

namespace GameLib.Demo.Wpf.Store;

public class NavigationStore
{
    public event Action? CurrentViewModelChanged;

    private ViewModelBase? _currentViewModel;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            _currentViewModel?.Dispose();
            _currentViewModel = value;
            OnCurrentViewModelChanged();
        }
    }

    private void OnCurrentViewModelChanged()
    {
        CurrentViewModelChanged?.Invoke();
    }
}
