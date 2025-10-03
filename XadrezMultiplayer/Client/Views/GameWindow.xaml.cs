using Client.Services;
using Client.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Client.Views
{
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetService<GameViewModel>() ?? new GameViewModel(App.Services.GetService<INetworkClient>()!, WeakReferenceMessenger.Default);
        }
    }
}