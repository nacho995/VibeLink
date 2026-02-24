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
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
            return new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:5093/")
            };
        });

        builder.Services.AddSingleton<ApiService>();

        // ===== VIEWMODELS =====
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<SwipeViewModel>();
        builder.Services.AddTransient<ContentViewModel>();
        builder.Services.AddTransient<MatchesViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<PremiumViewModel>();

        // ===== PAGES =====
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
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
