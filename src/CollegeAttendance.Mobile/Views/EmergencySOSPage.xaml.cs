using CollegeAttendance.Mobile.ViewModels;

namespace CollegeAttendance.Mobile.Views;

public partial class EmergencySOSPage : ContentPage
{
    private readonly EmergencySOSViewModel _viewModel;

    public EmergencySOSPage(EmergencySOSViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadDataCommand.Execute(null);
    }
}
