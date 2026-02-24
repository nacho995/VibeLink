using VibeLink.App.ViewModels;

namespace VibeLink.App.Views;

public partial class MatchesPage : ContentPage
{
    private readonly MatchesViewModel _viewModel;

    public MatchesPage(MatchesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadMatchesAsync();
    }
}
