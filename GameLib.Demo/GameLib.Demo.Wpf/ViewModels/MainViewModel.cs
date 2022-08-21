using CommunityToolkit.Mvvm.ComponentModel;

namespace GameLib.Demo.Wpf.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    NavigationBarViewModel _navigationBarViewModel = default!;

    public MainViewModel()
    {
        NavigationBarViewModel = new NavigationBarViewModel();
    }
}
