using VibeLink.App.ViewModels;

namespace VibeLink.App.Views;

/// <summary>
/// Code-behind de LoginPage.
/// En MVVM, el code-behind es mínimo: solo conecta la View con su ViewModel.
/// Toda la lógica está en LoginViewModel.
/// </summary>
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        // Esto conecta el XAML con el ViewModel
        // Ahora {Binding Email} en XAML apunta a viewModel.Email
        BindingContext = viewModel;
    }
}
