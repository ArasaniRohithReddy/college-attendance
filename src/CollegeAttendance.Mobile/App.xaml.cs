using CollegeAttendance.Mobile.Services;

namespace CollegeAttendance.Mobile;

public partial class App : Application
{
	private readonly AuthService _authService;

	public App(AuthService authService)
	{
		InitializeComponent();
		_authService = authService;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var shell = new AppShell();

		// Check auth state on startup — navigate to login if needed
		shell.Loaded += async (_, _) =>
		{
			var restored = await _authService.TryRestoreSessionAsync();
			if (!restored)
				await Shell.Current.GoToAsync("//login");
		};

		return new Window(shell);
	}
}