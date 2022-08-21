using CommunityToolkit.Mvvm.ComponentModel;

namespace GameLib.Demo.Wpf.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private NavigationBarViewModel _navigationBarViewModel = default!;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    public MainViewModel()
    {
        NavigationBarViewModel = new NavigationBarViewModel();
        CurrentViewModel = new LauncherViewModel();
    }
}
