namespace CollegeAttendance.Mobile.Services;

public class AuthTokenStore
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string ExpiresAtKey = "token_expires_at";

    public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
        await SecureStorage.Default.SetAsync(ExpiresAtKey, expiresAt.ToString("O"));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(AccessTokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(RefreshTokenKey);
    }

    public async Task<bool> IsTokenValidAsync()
    {
        var expiresStr = await SecureStorage.Default.GetAsync(ExpiresAtKey);
        if (string.IsNullOrEmpty(expiresStr)) return false;
        return DateTime.TryParse(expiresStr, out var expires) && expires > DateTime.UtcNow.AddMinutes(1);
    }

    public void ClearTokens()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(ExpiresAtKey);
    }
}
