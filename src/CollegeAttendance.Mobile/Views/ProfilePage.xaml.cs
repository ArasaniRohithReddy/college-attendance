namespace CollegeAttendance.Mobile.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ViewModels.ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.ProfileViewModel vm)
            vm.LoadProfileCommand.Execute(null);
    }
}
