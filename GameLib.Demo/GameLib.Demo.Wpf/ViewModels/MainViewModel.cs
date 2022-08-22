using CommunityToolkit.Mvvm.ComponentModel;
using GameLib.Demo.Wpf.Store;

namespace GameLib.Demo.Wpf.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;

    [ObservableProperty]
    private NavigationBarViewModel _navigationBarViewModel = default!;

    public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

    public MainViewModel(
        NavigationStore navigationStore,
        NavigationBarViewModel navigationBarViewModel)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        NavigationBarViewModel = navigationBarViewModel;
    }

    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }

    public override void Dispose()
    {
        _navigationStore.CurrentViewModelChanged -= OnCurrentViewModelChanged;
        base.Dispose();
    }
}
