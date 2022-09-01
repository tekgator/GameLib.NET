using System;

namespace GameLib.Demo.Wpf.ViewModels;

public class NavigationBarMenuItemViewModel
{
    public string ImageSource { get; set; } = default!;
    public string Text { get; set; } = default!;
    public Action Navigate { get; set; } = default!;
}
