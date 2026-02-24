using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VibeLink.App.Services;

namespace VibeLink.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SessionService _sessionService;

    public LoginViewModel(ApiService apiService, SessionService sessionService)
    {
        _apiService = apiService;
        _sessionService = sessionService;
    }

    [ObservableProperty] private string email = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string errorMessage = "";
    [ObservableProperty] private bool showError;
    [ObservableProperty] private bool isBusy;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Completa todos los campos";
            ShowError = true;
            return;
        }

        IsBusy = true;
        ShowError = false;

        var (success, message) = await _apiService.LoginAsync(Email, Password);

        if (success)
        {
            // Tras login, obtener el username del token y buscar el userId
            var username = await _sessionService.GetUsernameFromTokenAsync();
            if (username != null)
            {
                _sessionService.CurrentUsername = username;

                // Buscar el usuario por username para obtener su ID
                var users = await _apiService.GetUsersAsync();
                var currentUser = users.FirstOrDefault(u => u.Username == username);
                if (currentUser != null)
                {
                    _sessionService.CurrentUserId = currentUser.Id;
                }
            }

            await Shell.Current.GoToAsync("//HomePage");
        }
        else
        {
            ErrorMessage = message;
            ShowError = true;
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task GoToRegister()
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }
}
