using CollegeAttendance.Mobile.Models;

namespace CollegeAttendance.Mobile.Services;

public class AuthService
{
    private readonly ApiService _api;
    private readonly AuthTokenStore _tokenStore;

    public UserDto? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public event Action? AuthStateChanged;

    public AuthService(ApiService api, AuthTokenStore tokenStore)
    {
        _api = api;
        _tokenStore = tokenStore;
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        if (!await _tokenStore.IsTokenValidAsync())
        {
            var refreshToken = await _tokenStore.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            try
            {
                var authResponse = await _api.RefreshAsync(refreshToken);
                await _tokenStore.SaveTokensAsync(authResponse.AccessToken, authResponse.RefreshToken, authResponse.ExpiresAt);
                CurrentUser = authResponse.User;
                AuthStateChanged?.Invoke();
                return true;
            }
            catch
            {
                _tokenStore.ClearTokens();
                return false;
            }
        }

        try
        {
            CurrentUser = await _api.GetMeAsync();
            AuthStateChanged?.Invoke();
            return true;
        }
        catch
        {
            _tokenStore.ClearTokens();
            return false;
        }
    }

    public async Task<bool> LoginWithGoogleAsync(string idToken)
    {
        var authResponse = await _api.GoogleLoginAsync(idToken);
        await _tokenStore.SaveTokensAsync(authResponse.AccessToken, authResponse.RefreshToken, authResponse.ExpiresAt);
        CurrentUser = authResponse.User;
        AuthStateChanged?.Invoke();
        return true;
    }

    public async Task LogoutAsync()
    {
        try { await _api.LogoutAsync(); } catch { /* ignore */ }
        _tokenStore.ClearTokens();
        CurrentUser = null;
        AuthStateChanged?.Invoke();
    }
}
