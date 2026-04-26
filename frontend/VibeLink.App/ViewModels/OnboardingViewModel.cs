using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VibeLink.App.Models;
using VibeLink.App.Services;

namespace VibeLink.App.ViewModels;

/// <summary>
/// ViewModel del Onboarding: donde el usuario elige sus gustos por primera vez.
/// Combina carrusel por categorías, búsqueda y swipe.
/// </summary>
public partial class OnboardingViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SessionService _sessionService;

    public OnboardingViewModel(ApiService apiService, SessionService sessionService)
    {
        _apiService = apiService;
        _sessionService = sessionService;
    }

    // Contenido por categoría
    public ObservableCollection<DiscoverContentItem> Movies { get; } = [];
    public ObservableCollection<DiscoverContentItem> Series { get; } = [];
    public ObservableCollection<DiscoverContentItem> Games { get; } = [];
    public ObservableCollection<DiscoverContentItem> SearchResults { get; } = [];

    // Contenido seleccionado (liked)
    public ObservableCollection<DiscoverContentItem> LikedItems { get; } = [];

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isSearching;
    [ObservableProperty] private string searchQuery = "";
    [ObservableProperty] private string selectedCategory = "All"; // All, Movies, Series, Games
    [ObservableProperty] private int likedCount;
    [ObservableProperty] private int currentPage = 1;

    // El contenido que se muestra actualmente en el swipe card
    [ObservableProperty] private DiscoverContentItem? currentItem;
    [ObservableProperty] private int currentIndex;

    // Lista combinada para swipe mode
    public ObservableCollection<DiscoverContentItem> SwipeQueue { get; } = [];

    /// <summary>
    /// Carga contenido popular de las 3 categorías.
    /// </summary>
    [RelayCommand]
    public async Task LoadContentAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        var discovery = await _apiService.GetDiscoveryAsync(CurrentPage);

        Movies.Clear();
        foreach (var m in discovery.Movies) Movies.Add(m);

        Series.Clear();
        foreach (var s in discovery.Series) Series.Add(s);

        Games.Clear();
        foreach (var g in discovery.Games) Games.Add(g);

        // Llenar la cola de swipe mezclando contenido
        RefreshSwipeQueue();

        IsBusy = false;
    }

    /// <summary>
    /// Carga más contenido (siguiente página).
    /// </summary>
    [RelayCommand]
    public async Task LoadMoreAsync()
    {
        CurrentPage++;
        
        var discovery = await _apiService.GetDiscoveryAsync(CurrentPage);
        
        foreach (var m in discovery.Movies) Movies.Add(m);
        foreach (var s in discovery.Series) Series.Add(s);
        foreach (var g in discovery.Games) Games.Add(g);

        RefreshSwipeQueue();
    }

    /// <summary>
    /// Búsqueda de contenido.
    /// </summary>
    [RelayCommand]
    public async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) 
        {
            IsSearching = false;
            SearchResults.Clear();
            return;
        }

        IsSearching = true;
        var results = await _apiService.SearchContentAsync(SearchQuery);
        
        SearchResults.Clear();
        foreach (var r in results) SearchResults.Add(r);
    }

    /// <summary>
    /// Like a un contenido (tap en el carrusel o swipe derecha).
    /// </summary>
    [RelayCommand]
    public async Task LikeContentAsync(DiscoverContentItem item)
    {
        if (item == null) return;

        // Añadir a la lista de liked visualmente
        if (!LikedItems.Any(l => l.ExternalId == item.ExternalId))
        {
            LikedItems.Add(item);
            LikedCount = LikedItems.Count;
        }

        // Enviar al backend usando el endpoint /external que auto-crea el contenido
        var userId = _sessionService.CurrentUserId;
        await _apiService.LikeExternalAsync(new ExternalLikeRequest
        {
            UserId = userId,
            ExternalId = item.ExternalId,
            Title = item.Title,
            ImageUrl = item.ImageUrl,
            State = ContentState.Liked
        });

        // Avanzar al siguiente en swipe mode
        AdvanceSwipeQueue();
    }

    /// <summary>
    /// Dislike a un contenido (swipe izquierda).
    /// </summary>
    [RelayCommand]
    public async Task DislikeContentAsync(DiscoverContentItem item)
    {
        if (item == null) return;

        var userId = _sessionService.CurrentUserId;
        await _apiService.LikeExternalAsync(new ExternalLikeRequest
        {
            UserId = userId,
            ExternalId = item.ExternalId,
            Title = item.Title,
            ImageUrl = item.ImageUrl,
            State = ContentState.Disliked
        });

        AdvanceSwipeQueue();
    }

    /// <summary>
    /// Quitar like de un contenido.
    /// </summary>
    [RelayCommand]
    public void RemoveLike(DiscoverContentItem item)
    {
        var existing = LikedItems.FirstOrDefault(l => l.ExternalId == item.ExternalId);
        if (existing != null)
        {
            LikedItems.Remove(existing);
            LikedCount = LikedItems.Count;
        }
    }

    /// <summary>
    /// Cambiar categoría del filtro.
    /// </summary>
    [RelayCommand]
    public void SetCategory(string category)
    {
        SelectedCategory = category;
        RefreshSwipeQueue();
    }

    /// <summary>
    /// Continuar al siguiente paso (ir a la app principal).
    /// Se requiere un mínimo de gustos seleccionados.
    /// </summary>
    [RelayCommand]
    public async Task ContinueAsync()
    {
        if (LikedCount < 5)
        {
            await Shell.Current.DisplayAlert(
                "Elige más contenido",
                "Selecciona al menos 5 películas, series o juegos para que podamos encontrar matches compatibles.",
                "OK");
            return;
        }

        // Navegar a la página principal
        await Shell.Current.GoToAsync("//main");
    }

    /// <summary>
    /// Mezcla contenido de las categorías para el modo swipe.
    /// </summary>
    private void RefreshSwipeQueue()
    {
        SwipeQueue.Clear();

        var items = SelectedCategory switch
        {
            "Movies" => Movies.ToList(),
            "Series" => Series.ToList(),
            "Games" => Games.ToList(),
            _ => Movies.Concat(Series).Concat(Games).ToList()
        };

        // Mezclar aleatoriamente
        var shuffled = items.OrderBy(_ => Random.Shared.Next()).ToList();
        
        // Excluir los que ya tienen like
        var likedIds = LikedItems.Select(l => l.ExternalId).ToHashSet();
        foreach (var item in shuffled.Where(i => !likedIds.Contains(i.ExternalId)))
            SwipeQueue.Add(item);

        CurrentIndex = 0;
        CurrentItem = SwipeQueue.FirstOrDefault();
    }

    /// <summary>
    /// Avanza al siguiente item en la cola de swipe.
    /// </summary>
    private async void AdvanceSwipeQueue()
    {
        CurrentIndex++;
        if (CurrentIndex < SwipeQueue.Count)
        {
            CurrentItem = SwipeQueue[CurrentIndex];
        }
        else
        {
            // Cargar más contenido
            await LoadMoreAsync();
        }
    }
}
