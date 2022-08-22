using GameLib.Demo.Wpf.ViewModels;

namespace GameLib.Demo.Wpf.Services;

public interface INavigationService<TViewModel>
    where TViewModel : ViewModelBase
{
    void Navigate();
}
