using CollegeAttendance.Mobile.ViewModels;

namespace CollegeAttendance.Mobile.Views;

public partial class SessionDetailPage : ContentPage
{
    public SessionDetailPage(SessionDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
