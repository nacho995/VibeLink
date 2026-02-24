using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VibeLink.App.Models;
using VibeLink.App.Services;

namespace VibeLink.App.ViewModels;

/// <summary>
/// ViewModel de la pantalla de Swipe.
/// Carga usuarios ordenados por compatibilidad y permite Like/Dislike.
/// Funciona como Tinder: muestra un usuario a la vez.
/// </summary>
public partial class SwipeViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SessionService _sessionService;
    private List<CompatibilityResult> _users = [];
    private int _currentIndex;

    public SwipeViewModel(ApiService apiService, SessionService sessionService)
    {
        _apiService = apiService;
        _sessionService = sessionService;
    }

    [ObservableProperty] private string currentUsername = "";
    [ObservableProperty] private string currentBio = "";
    [ObservableProperty] private string currentAvatar = "";
    [ObservableProperty] private int currentCompatibility;
    [ObservableProperty] private bool hasUsers;
    [ObservableProperty] private bool noMoreUsers;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string matchMessage = "";
    [ObservableProperty] private bool showMatch;

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        IsBusy = true;
        ShowMatch = false;
        _users = await _apiService.GetCompatibilityAsync(_sessionService.CurrentUserId);
        _currentIndex = 0;
        ShowCurrentUser();
        IsBusy = false;
    }

    private void ShowCurrentUser()
    {
        if (_currentIndex < _users.Count && _users[_currentIndex].Usuario != null)
        {
            var user = _users[_currentIndex].Usuario!;
            CurrentUsername = user.Username;
            CurrentBio = user.Bio ?? "Sin bio";
            CurrentAvatar = user.AvatarUrl ?? "";
            CurrentCompatibility = _users[_currentIndex].Compatibilidad;
            HasUsers = true;
            NoMoreUsers = false;
        }
        else
        {
            HasUsers = false;
            NoMoreUsers = true;
        }
    }

    [RelayCommand]
    private async Task LikeAsync()
    {
        await DoSwipe(SwipeState.Like);
    }

    [RelayCommand]
    private async Task DislikeAsync()
    {
        await DoSwipe(SwipeState.Dislike);
    }

    private async Task DoSwipe(SwipeState state)
    {
        if (_currentIndex >= _users.Count || _users[_currentIndex].Usuario == null) return;

        ShowMatch = false;
        var targetUserId = _users[_currentIndex].Usuario!.Id;

        var request = new PeopleSwipeRequest
        {
            UserId = _sessionService.CurrentUserId,
            MatchingUserId = targetUserId,
            State = state
        };

        var (success, isMatch, message) = await _apiService.SwipePersonAsync(request);

        if (success && isMatch)
        {
            MatchMessage = $"Match con {CurrentUsername}!";
            ShowMatch = true;
        }
        else if (!success)
        {
            // Posiblemente sin swipes restantes
            await Shell.Current.DisplayAlertAsync("Swipe", message, "OK");
        }

        _currentIndex++;
        ShowCurrentUser();
    }
}
