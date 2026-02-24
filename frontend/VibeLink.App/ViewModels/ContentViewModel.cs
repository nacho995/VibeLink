using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VibeLink.App.Models;
using VibeLink.App.Services;

namespace VibeLink.App.ViewModels;

/// <summary>
/// Pantalla para hacer swipe sobre contenido (películas, series, videojuegos).
/// Dar Like a contenido mejora tu % de compatibilidad con otros usuarios
/// que les gusta lo mismo.
/// </summary>
public partial class ContentViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SessionService _sessionService;
    private List<Content> _contents = [];
    private int _currentIndex;

    public ContentViewModel(ApiService apiService, SessionService sessionService)
    {
        _apiService = apiService;
        _sessionService = sessionService;
    }

    [ObservableProperty] private string currentTitle = "";
    [ObservableProperty] private string currentImage = "";
    [ObservableProperty] private string currentType = "";
    [ObservableProperty] private string currentGenres = "";
    [ObservableProperty] private int currentYear;
    [ObservableProperty] private bool hasContent;
    [ObservableProperty] private bool noMoreContent;
    [ObservableProperty] private bool isBusy;

    [RelayCommand]
    public async Task LoadContentAsync()
    {
        IsBusy = true;
        _contents = await _apiService.GetContentAsync();
        _currentIndex = 0;
        ShowCurrentContent();
        IsBusy = false;
    }

    private void ShowCurrentContent()
    {
        if (_currentIndex < _contents.Count)
        {
            var c = _contents[_currentIndex];
            CurrentTitle = c.Titulo;
            CurrentImage = c.ImagenUrl ?? "";
            CurrentType = c.Type switch
            {
                Models.ContentType.pelicula => "Película",
                Models.ContentType.serie => "Serie",
                Models.ContentType.videojuego => "Videojuego",
                _ => ""
            };
            CurrentGenres = c.Generos != null ? string.Join(", ", c.Generos) : "";
            CurrentYear = c.Año;
            HasContent = true;
            NoMoreContent = false;
        }
        else
        {
            HasContent = false;
            NoMoreContent = true;
        }
    }

    [RelayCommand]
    private async Task LikeContentAsync()
    {
        await DoContentSwipe(ContentState.Liked);
    }

    [RelayCommand]
    private async Task DislikeContentAsync()
    {
        await DoContentSwipe(ContentState.Disliked);
    }

    private async Task DoContentSwipe(ContentState state)
    {
        if (_currentIndex >= _contents.Count) return;

        var request = new ContentSwipeRequest
        {
            UserId = _sessionService.CurrentUserId,
            ContentId = _contents[_currentIndex].Id,
            State = state,
            Punctuation = state == ContentState.Liked ? 10 : 0
        };

        var (success, message) = await _apiService.SwipeContentAsync(request);
        if (!success)
        {
            await Shell.Current.DisplayAlertAsync("Error", message, "OK");
        }

        _currentIndex++;
        ShowCurrentContent();
    }
}
