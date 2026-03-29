using CollegeAttendance.Mobile.Views;

namespace CollegeAttendance.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Register routes for pages navigated to programmatically
		Routing.RegisterRoute("session", typeof(SessionDetailPage));
		Routing.RegisterRoute("notifications", typeof(NotificationsPage));
		Routing.RegisterRoute("login", typeof(LoginPage));
		Routing.RegisterRoute("gamification", typeof(GamificationPage));
		Routing.RegisterRoute("leave", typeof(LeavePage));
		Routing.RegisterRoute("emergency", typeof(EmergencySOSPage));
	}
}
