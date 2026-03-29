namespace CollegeAttendance.Mobile.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage(ViewModels.NotificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.NotificationsViewModel vm)
            vm.LoadDataCommand.Execute(null);
    }
}
