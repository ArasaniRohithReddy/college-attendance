namespace CollegeAttendance.Mobile.Views;

public partial class OutingPage : ContentPage
{
    public OutingPage(ViewModels.OutingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.OutingViewModel vm)
            vm.LoadDataCommand.Execute(null);
    }
}
