using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VibeLink.App.Services;

namespace VibeLink.App.ViewModels;

/// <summary>
/// Pantalla para hacerse Premium via Stripe.
/// Crea una sesión de checkout y abre el navegador para pagar.
/// </summary>
public partial class PremiumViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SessionService _sessionService;

    public PremiumViewModel(ApiService apiService, SessionService sessionService)
    {
        _apiService = apiService;
        _sessionService = sessionService;
    }

    [ObservableProperty] private bool isPremium;
    [ObservableProperty] private bool isBusy;

    [RelayCommand]
    public async Task CheckStatusAsync()
    {
        var user = await _apiService.GetUserAsync(_sessionService.CurrentUserId);
        IsPremium = user?.IsPremium ?? false;
    }

    [RelayCommand]
    private async Task BuyPremiumAsync()
    {
        IsBusy = true;

        var checkoutUrl = await _apiService.CreateCheckoutAsync(_sessionService.CurrentUserId);
        if (!string.IsNullOrEmpty(checkoutUrl))
        {
            // Abre el navegador del dispositivo con la página de pago de Stripe
            await Browser.OpenAsync(checkoutUrl, BrowserLaunchMode.SystemPreferred);
        }
        else
        {
            await Shell.Current.DisplayAlertAsync("Error",
                "No se pudo crear la sesión de pago", "OK");
        }

        IsBusy = false;
    }
}
