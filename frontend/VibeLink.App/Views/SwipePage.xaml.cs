using VibeLink.App.ViewModels;

namespace VibeLink.App.Views;

public partial class SwipePage : ContentPage
{
    private readonly SwipeViewModel _viewModel;

    public SwipePage(SwipeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadUsersAsync();
    }
}
