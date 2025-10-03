using System.Windows;
using Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetService<MainViewModel>();
    }
}