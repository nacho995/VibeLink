using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VibeLink.App.Services;

namespace VibeLink.App.ViewModels;

/// <summary>
/// ViewModel de la pantalla de Registro.
/// Mismo patrón que LoginViewModel pero con más campos.
/// </summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    public RegisterViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [ObservableProperty]
    private string username = "";

    [ObservableProperty]
    private string email = "";

    [ObservableProperty]
    private string password = "";

    [ObservableProperty]
    private string confirmPassword = "";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool showError;

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        // Validaciones del lado del cliente
        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Completa todos los campos";
            ShowError = true;
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Las contraseñas no coinciden";
            ShowError = true;
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "La contraseña debe tener al menos 6 caracteres";
            ShowError = true;
            return;
        }

        IsBusy = true;
        ShowError = false;

        var (success, message) = await _apiService.RegisterAsync(
            Username, Email, Password, ConfirmPassword);

        if (success)
        {
            // Registro exitoso: volver al login para que se loguee
            await Shell.Current.DisplayAlertAsync("Registro exitoso",
                "Tu cuenta ha sido creada. Inicia sesión.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            ErrorMessage = message;
            ShowError = true;
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task GoToLogin()
    {
        // ".." significa "volver atrás" en la navegación de Shell
        await Shell.Current.GoToAsync("..");
    }
}
