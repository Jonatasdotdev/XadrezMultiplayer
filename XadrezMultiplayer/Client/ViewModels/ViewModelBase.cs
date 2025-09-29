using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Client.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _statusMessage = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    protected void SetStatus(string message)
    {
        StatusMessage = message;
    }
}