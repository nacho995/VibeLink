using VibeLink.App.ViewModels;

namespace VibeLink.App.Views;

public partial class ContentSwipePage : Microsoft.Maui.Controls.ContentPage
{
    private readonly ContentViewModel _viewModel;

    public ContentSwipePage(ContentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadContentAsync();
    }
}
