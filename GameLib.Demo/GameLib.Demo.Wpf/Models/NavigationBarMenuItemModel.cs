using System;

namespace GameLib.Demo.Wpf.Models;

public class NavigationBarMenuItemModel
{
    public string ImageSource { get; set; } = default!;
    public string Text { get; set; } = default!;
    public Action Navigate { get; set; } = default!;
}
