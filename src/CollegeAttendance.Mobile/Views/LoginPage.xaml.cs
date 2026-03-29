using CollegeAttendance.Mobile.ViewModels;

namespace CollegeAttendance.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LoginViewModel vm)
            await vm.CheckExistingSessionCommand.ExecuteAsync(null);
    }
}
