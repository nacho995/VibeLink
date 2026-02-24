using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VibeLink.App.Models;
using VibeLink.App.Services;

namespace VibeLink.App.ViewModels;

/// <summary>
/// Lista de todos tus matches. Al pulsar uno, abre el chat.
/// ObservableCollection notifica a la UI cuando cambia la lista.
/// </summary>
public partial class MatchesViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SessionService _sessionService;

    public MatchesViewModel(ApiService apiService, SessionService sessionService)
    {
        _apiService = apiService;
        _sessionService = sessionService;
    }

    public ObservableCollection<MatchInfo> Matches { get; } = [];

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isEmpty;

    [RelayCommand]
    public async Task LoadMatchesAsync()
    {
        IsBusy = true;
        Matches.Clear();

        var matches = await _apiService.GetMyMatchesAsync(_sessionService.CurrentUserId);
        foreach (var m in matches)
            Matches.Add(m);

        IsEmpty = Matches.Count == 0;
        IsBusy = false;
    }

    [RelayCommand]
    private async Task OpenChatAsync(MatchInfo match)
    {
        if (match?.AnotherUser == null) return;

        // Navegar al chat pasando los IDs como parámetros en la URL
        await Shell.Current.GoToAsync(
            $"ChatPage?userId={_sessionService.CurrentUserId}&matchUserId={match.AnotherUser.Id}&matchUsername={match.AnotherUser.Username}");
    }
}
