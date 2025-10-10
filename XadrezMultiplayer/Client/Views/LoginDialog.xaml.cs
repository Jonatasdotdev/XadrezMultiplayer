using Client.ViewModels;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Client.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace Client.Views
{
    public partial class LoginDialog : Window
    {
        public LoginDialog()
        {
            InitializeComponent();
            DataContext = App.Services.GetService<LoginViewModel>() ??
                          new LoginViewModel(
                              App.Services.GetService<INetworkClient>()!,
                              WeakReferenceMessenger.Default,
                              App.Services.GetService<DialogService>()!);
        }
    }
}