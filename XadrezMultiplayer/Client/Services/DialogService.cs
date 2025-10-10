using Client.ViewModels;
using Client.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Client.Services
{
    public class DialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowLoginDialog()
        {
            var loginDialog = _serviceProvider.GetService<LoginDialog>();
            loginDialog?.ShowDialog();
        }

        public void ShowRegisterDialog()
        {
            var registerDialog = _serviceProvider.GetService<RegisterDialog>();
            registerDialog?.ShowDialog();
        }

        public void CloseCurrentDialog(Window dialog)
        {
            dialog?.Close();
        }
    }
    
}