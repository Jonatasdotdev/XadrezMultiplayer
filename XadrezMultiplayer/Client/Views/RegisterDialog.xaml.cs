using Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using Client.Services;


namespace Client.Views
{
    public partial class RegisterDialog : Window
    {
        public RegisterDialog()
        {
            InitializeComponent();
            DataContext = App.Services.GetService<RegisterViewModel>() ?? 
                          new RegisterViewModel(App.Services.GetService<INetworkClient>()!, WeakReferenceMessenger.Default);
        }
    }
}