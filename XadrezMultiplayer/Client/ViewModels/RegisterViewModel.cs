using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private readonly AuthService _authService;

    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }

    public ICommand RegisterCommand { get; }

    public RegisterViewModel(AuthService authService)
    {
        _authService = authService;
        RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
    }

    private async Task RegisterAsync()
    {
        await _authService.RegisterAsync(Username, Password, Email);
    }
}
