using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Services;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginWithGoogleAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = null;

        try
        {
#if ANDROID
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri($"{AppConstants.ApiBaseUrl}api/auth/google-mobile"),
                new Uri("collegeattendance://callback"));

            var idToken = authResult.Properties.TryGetValue("id_token", out var token) ? token : null;
#else
            // For other platforms, use a WebView-based approach
            string? idToken = null;
            await Shell.Current.DisplayAlert("Info", "Google Sign-In is configured for Android. On other platforms, use the web app.", "OK");
#endif
            if (string.IsNullOrEmpty(idToken))
            {
                ErrorMessage = "Google sign-in failed. Please try again.";
                return;
            }

            await _authService.LoginWithGoogleAsync(idToken);
            await Shell.Current.GoToAsync("//main");
        }
        catch (TaskCanceledException)
        {
            // User cancelled
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CheckExistingSessionAsync()
    {
        IsBusy = true;
        try
        {
            if (await _authService.TryRestoreSessionAsync())
            {
                await Shell.Current.GoToAsync("//main");
            }
        }
        catch
        {
            // No valid session
        }
        finally
        {
            IsBusy = false;
        }
    }
}
