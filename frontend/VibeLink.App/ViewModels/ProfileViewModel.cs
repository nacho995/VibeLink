using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VibeLink.App.Models;
using VibeLink.App.Services;

namespace VibeLink.App.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SessionService _sessionService;

    public ProfileViewModel(ApiService apiService, SessionService sessionService)
    {
        _apiService = apiService;
        _sessionService = sessionService;
    }

    [ObservableProperty] private string username = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string bio = "";
    [ObservableProperty] private string avatarUrl = "";
    [ObservableProperty] private DateTime dateOfBirth = DateTime.Now.AddYears(-18);
    [ObservableProperty] private int selectedGenderIndex;
    [ObservableProperty] private bool isPremium;
    [ObservableProperty] private int swipesLeft;
    [ObservableProperty] private string errorMessage = "";
    [ObservableProperty] private bool showError;
    [ObservableProperty] private bool isBusy;

    public List<string> GenderOptions { get; } = ["Male", "Female"];

    /// <summary>
    /// Carga los datos del perfil desde el backend.
    /// Se llama cuando se abre la pantalla.
    /// </summary>
    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        IsBusy = true;
        var user = await _apiService.GetUserAsync(_sessionService.CurrentUserId);
        if (user != null)
        {
            Username = user.Username;
            Email = user.Email;
            Bio = user.Bio ?? "";
            AvatarUrl = user.AvatarUrl ?? "";
            DateOfBirth = user.DateOfBirth;
            SelectedGenderIndex = user.Gender == "Female" ? 1 : 0;
            IsPremium = user.IsPremium;
            SwipesLeft = user.Swipes;
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        IsBusy = true;
        ShowError = false;

        var profile = new ProfileUpdateRequest
        {
            AvatarUrl = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl,
            Gender = SelectedGenderIndex, // 0=Male, 1=Female (coincide con el enum del backend)
            DateOfBirth = DateOfBirth,
            Bio = Bio
        };

        var (success, message) = await _apiService.UpdateProfileAsync(
            _sessionService.CurrentUserId, profile);

        if (success)
        {
            await Shell.Current.DisplayAlertAsync("Perfil", "Perfil actualizado correctamente", "OK");
        }
        else
        {
            ErrorMessage = message;
            ShowError = true;
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _sessionService.Logout();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
