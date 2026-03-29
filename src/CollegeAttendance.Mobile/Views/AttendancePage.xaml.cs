using CollegeAttendance.Mobile.ViewModels;

namespace CollegeAttendance.Mobile.Views;

public partial class AttendancePage : ContentPage
{
    public AttendancePage(AttendanceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AttendanceViewModel vm)
            await vm.LoadDataCommand.ExecuteAsync(null);
    }
}
