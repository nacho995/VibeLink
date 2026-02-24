using VibeLink.App.Views;

namespace VibeLink.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rutas para navegación por código (páginas que no están en tabs)
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("ChatPage", typeof(ChatPage));
    }
}
