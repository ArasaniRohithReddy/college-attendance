namespace CollegeAttendance.Mobile.Views;

public partial class LeavePage : ContentPage
{
    public LeavePage(ViewModels.LeaveViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.LeaveViewModel vm)
            vm.LoadDataCommand.Execute(null);
    }
}
