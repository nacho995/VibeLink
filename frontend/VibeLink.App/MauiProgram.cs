using Microsoft.Extensions.Logging;
using VibeLink.App.Services;
using VibeLink.App.ViewModels;
using VibeLink.App.Views;

namespace VibeLink.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ===== SERVICIOS =====
        builder.Services.AddSingleton<TokenService>();
        builder.Services.AddSingleton<SessionService>();

        builder.Services.AddSingleton(sp =>
        {
            var handler = new HttpClientHandler();
            
#if DEBUG
            // En desarrollo, aceptar certificados self-signed y usar localhost
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            var baseUrl = DeviceInfo.Platform == DevicePlatform.Android 
                ? "http://10.0.2.2:5093/"  // Android Emulator -> localhost
                : "http://localhost:5093/"; // iOS Simulator / Mac
#else
            // En producción, usar la URL de Fly.io
            var baseUrl = "https://vibelink-api.fly.dev/";
#endif
            
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        });

        builder.Services.AddSingleton<ApiService>();

        // ===== VIEWMODELS =====
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<SwipeViewModel>();
        builder.Services.AddTransient<ContentViewModel>();
        builder.Services.AddTransient<MatchesViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<PremiumViewModel>();

        // ===== PAGES =====
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<SwipePage>();
        builder.Services.AddTransient<ContentSwipePage>();
        builder.Services.AddTransient<MatchesPage>();
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<PremiumPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
