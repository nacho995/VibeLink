using VibeLink.App.ViewModels;

namespace VibeLink.App.Views;

public partial class PremiumPage : ContentPage
{
    private readonly PremiumViewModel _viewModel;

    public PremiumPage(PremiumViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CheckStatusAsync();
    }
}
