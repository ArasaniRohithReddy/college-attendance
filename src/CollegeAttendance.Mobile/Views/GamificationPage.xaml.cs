namespace CollegeAttendance.Mobile.Views;

public partial class GamificationPage : ContentPage
{
    public GamificationPage(ViewModels.GamificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.GamificationViewModel vm)
            vm.LoadDataCommand.Execute(null);
    }
}
