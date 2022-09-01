using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace GameLib.Demo.Wpf.ViewModels;

public class ViewModelBase : ObservableObject, IDisposable
{
    public virtual void Dispose() { }
}
