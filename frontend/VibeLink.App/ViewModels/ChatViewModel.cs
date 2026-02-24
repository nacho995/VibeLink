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
    [ObservableProperty] private int compatibilityPercent = 0;
    
    // Propiedades calculadas para el header del chat
    public string MatchInitial => string.IsNullOrEmpty(MatchUsername) ? "?" : MatchUsername[0].ToString().ToUpper();
    public string CompatibilityText => CompatibilityPercent > 0 ? $"{CompatibilityPercent}% compatibilidad" : "Match";

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
        if (query.TryGetValue("compatibility", out var comp))
            CompatibilityPercent = int.Parse(comp.ToString()!);
        
        // Notificar que las propiedades calculadas han cambiado
        OnPropertyChanged(nameof(MatchInitial));
        OnPropertyChanged(nameof(CompatibilityText));
    }

    [RelayCommand]
    public async Task LoadMessagesAsync()
    {
        IsBusy = true;
        Messages.Clear();

        var messages = await _apiService.GetMessagesAsync(_userId, _matchUserId);
        foreach (var m in messages)
        {
            m.CurrentUserId = _userId; // Para que sepa quién es "yo"
            Messages.Add(m);
        }

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
                Date = DateTime.UtcNow,
                CurrentUserId = _userId
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
