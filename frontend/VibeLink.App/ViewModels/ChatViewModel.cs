using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VibeLink.App.Models;
using VibeLink.App.Services;

namespace VibeLink.App.ViewModels;

/// <summary>
/// Chat con un match. Recibe parámetros de navegación via QueryProperty.
/// IQueryAttributable permite recibir parámetros de la URL de navegación
/// (como ?userId=1 matchUserId=2 matchUsername=Ana).
/// </summary>
public partial class ChatViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int _userId;
    private int _matchUserId;

    public ChatViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public ObservableCollection<MessageChat> Messages { get; } = [];

    [ObservableProperty] private string matchUsername = "";
    [ObservableProperty] private string newMessage = "";
    [ObservableProperty] private bool isBusy;

    /// <summary>
    /// MAUI llama a esto automáticamente con los parámetros de la URL.
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("userId", out var uid))
            _userId = int.Parse(uid.ToString()!);
        if (query.TryGetValue("matchUserId", out var mid))
            _matchUserId = int.Parse(mid.ToString()!);
        if (query.TryGetValue("matchUsername", out var name))
            MatchUsername = name.ToString()!;
    }

    [RelayCommand]
    public async Task LoadMessagesAsync()
    {
        IsBusy = true;
        Messages.Clear();

        var messages = await _apiService.GetMessagesAsync(_userId, _matchUserId);
        foreach (var m in messages)
            Messages.Add(m);

        IsBusy = false;
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMessage)) return;

        var request = new SendMessageRequest
        {
            UserId = _userId,
            MatchingUserId = _matchUserId,
            Message = NewMessage.Trim()
        };

        var (success, _) = await _apiService.SendMessageAsync(request);
        if (success)
        {
            // Añadir el mensaje localmente para que aparezca inmediatamente
            Messages.Add(new MessageChat
            {
                UserId = _userId,
                MatchingUserId = _matchUserId,
                Message = NewMessage.Trim(),
                Date = DateTime.UtcNow
            });
            NewMessage = "";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadMessagesAsync();
    }
}
