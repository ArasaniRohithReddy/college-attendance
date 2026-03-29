using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using CollegeAttendance.Mobile.Services;
using CollegeAttendance.Mobile.ViewModels;
using CollegeAttendance.Mobile.Views;

namespace CollegeAttendance.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// HttpClient
		builder.Services.AddSingleton(sp =>
		{
			var client = new HttpClient { BaseAddress = new Uri(AppConstants.ApiBaseUrl) };
			client.DefaultRequestHeaders.Add("Accept", "application/json");
			return client;
		});

		// Services
		builder.Services.AddSingleton<AuthTokenStore>();
		builder.Services.AddSingleton<ApiService>();
		builder.Services.AddSingleton<AuthService>();
		builder.Services.AddSingleton<LocationService>();
		builder.Services.AddSingleton<ConnectivityService>();

		// ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<SessionDetailViewModel>();
		builder.Services.AddTransient<QRScanViewModel>();
		builder.Services.AddTransient<AttendanceViewModel>();
		builder.Services.AddTransient<OutingViewModel>();
		builder.Services.AddTransient<NotificationsViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<GamificationViewModel>();
		builder.Services.AddTransient<LeaveViewModel>();
		builder.Services.AddTransient<EmergencySOSViewModel>();

		// Pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<SessionDetailPage>();
		builder.Services.AddTransient<QRScanPage>();
		builder.Services.AddTransient<AttendancePage>();
		builder.Services.AddTransient<OutingPage>();
		builder.Services.AddTransient<NotificationsPage>();
		builder.Services.AddTransient<ProfilePage>();
		builder.Services.AddTransient<GamificationPage>();
		builder.Services.AddTransient<LeavePage>();
		builder.Services.AddTransient<EmergencySOSPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
