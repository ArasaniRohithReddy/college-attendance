namespace CollegeAttendance.Mobile;

public static class AppConstants
{
#if DEBUG
    // For Android emulator, use 10.0.2.2 which maps to host machine's localhost
    // For iOS simulator, use localhost
    // For physical device, use your machine's IP
    public static string ApiBaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5172/"
            : "http://localhost:5172/";
#else
    public static string ApiBaseUrl => "https://your-production-url.azurewebsites.net/";
#endif
}
