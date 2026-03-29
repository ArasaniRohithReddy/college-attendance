using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CollegeAttendance.Mobile.Models;

namespace CollegeAttendance.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly AuthTokenStore _tokenStore;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ApiService(HttpClient http, AuthTokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    private async Task EnsureAuthHeaderAsync()
    {
        var token = await _tokenStore.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // ===== Auth =====
    public async Task<AuthResponse> GoogleLoginAsync(string idToken)
    {
        var response = await _http.PostAsJsonAsync("api/auth/google", new GoogleLoginRequest { IdToken = idToken }, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions))!;
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var response = await _http.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest { RefreshToken = refreshToken }, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions))!;
    }

    public async Task<UserDto> GetMeAsync()
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<UserDto>("api/auth/me", JsonOptions))!;
    }

    public async Task LogoutAsync()
    {
        await EnsureAuthHeaderAsync();
        await _http.PostAsync("api/auth/logout", null);
    }

    // ===== Sessions =====
    public async Task<List<ClassSessionDto>> GetTodaySessionsAsync()
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<List<ClassSessionDto>>("api/classsessions/today", JsonOptions)) ?? [];
    }

    public async Task<ClassSessionDto> GetSessionAsync(Guid id)
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<ClassSessionDto>($"api/classsessions/{id}", JsonOptions))!;
    }

    // ===== QR =====
    public async Task<QRSessionDto> GenerateQRAsync(Guid classSessionId, int expirationSeconds = 30)
    {
        await EnsureAuthHeaderAsync();
        var req = new GenerateQRRequest { ClassSessionId = classSessionId, ExpirationSeconds = expirationSeconds };
        var response = await _http.PostAsJsonAsync("api/qr/generate", req, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QRSessionDto>(JsonOptions))!;
    }

    // ===== Attendance =====
    public async Task<AttendanceDto> MarkAttendanceAsync(MarkAttendanceRequest request)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/attendance/mark", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AttendanceDto>(JsonOptions))!;
    }

    public async Task<List<AttendanceDto>> GetMyAttendanceAsync(Guid studentId)
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<List<AttendanceDto>>($"api/attendance/student/{studentId}", JsonOptions)) ?? [];
    }

    public async Task<List<AttendanceReportDto>> GetAttendanceReportAsync(Guid courseId)
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<List<AttendanceReportDto>>($"api/attendance/report/{courseId}", JsonOptions)) ?? [];
    }

    // ===== Outing =====
    public async Task<List<OutingRequestDto>> GetMyOutingsAsync()
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<List<OutingRequestDto>>("api/outing/my", JsonOptions)) ?? [];
    }

    public async Task<OutingRequestDto> CreateOutingAsync(CreateOutingRequest request)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/outing", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OutingRequestDto>(JsonOptions))!;
    }

    public async Task<List<OutingRequestDto>> GetAllOutingsAsync(OutingStatus? status = null)
    {
        await EnsureAuthHeaderAsync();
        var url = status.HasValue ? $"api/outing?status={(int)status}" : "api/outing";
        return (await _http.GetFromJsonAsync<List<OutingRequestDto>>(url, JsonOptions)) ?? [];
    }

    public async Task ApproveOutingAsync(Guid id, string? remarks = null)
    {
        await EnsureAuthHeaderAsync();
        await _http.PostAsJsonAsync($"api/outing/{id}/approve", new { remarks }, JsonOptions);
    }

    public async Task RejectOutingAsync(Guid id, string remarks)
    {
        await EnsureAuthHeaderAsync();
        await _http.PostAsJsonAsync($"api/outing/{id}/reject", new { remarks }, JsonOptions);
    }

    // ===== Notifications =====
    public async Task<List<NotificationDto>> GetNotificationsAsync()
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<List<NotificationDto>>("api/notifications", JsonOptions)) ?? [];
    }

    public async Task<int> GetUnreadCountAsync()
    {
        await EnsureAuthHeaderAsync();
        return await _http.GetFromJsonAsync<int>("api/notifications/unread-count", JsonOptions);
    }

    public async Task MarkNotificationReadAsync(Guid id)
    {
        await EnsureAuthHeaderAsync();
        await _http.PutAsync($"api/notifications/{id}/read", null);
    }

    public async Task MarkAllNotificationsReadAsync()
    {
        await EnsureAuthHeaderAsync();
        await _http.PutAsync("api/notifications/read-all", null);
    }

    // ===== Analytics =====
    public async Task<DashboardAnalyticsDto> GetDashboardAsync()
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<DashboardAnalyticsDto>("api/analytics/dashboard", JsonOptions))!;
    }

    // ===== Gamification =====
    public async Task<GamificationDashboardDto> GetGamificationDashboardAsync()
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<GamificationDashboardDto>("api/gamification/dashboard", JsonOptions))!;
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(string period = "weekly", int top = 20)
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<List<LeaderboardEntryDto>>($"api/gamification/leaderboard?period={Uri.EscapeDataString(period)}&top={top}", JsonOptions)) ?? [];
    }

    // ===== Leave Management =====
    public async Task<PagedResult<LeaveRequestDto>> GetMyLeavesAsync(int page = 1, int pageSize = 20)
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<PagedResult<LeaveRequestDto>>($"api/leave/my?page={page}&pageSize={pageSize}", JsonOptions))!;
    }

    public async Task<LeaveRequestDto> CreateLeaveAsync(CreateLeaveRequest request)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/leave", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LeaveRequestDto>(JsonOptions))!;
    }

    public async Task CancelLeaveAsync(Guid id)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsync($"api/leave/{id}/cancel", null);
        response.EnsureSuccessStatusCode();
    }

    // ===== Emergency SOS =====
    public async Task<EmergencySOSDto> CreateSOSAsync(CreateSOSRequest request)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/emergency/sos", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EmergencySOSDto>(JsonOptions))!;
    }

    public async Task<List<EmergencySOSDto>> GetActiveSOSAsync()
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<List<EmergencySOSDto>>("api/emergency/active", JsonOptions)) ?? [];
    }

    public async Task<List<EmergencySOSDto>> GetMySOSAsync()
    {
        await EnsureAuthHeaderAsync();
        return (await _http.GetFromJsonAsync<List<EmergencySOSDto>>("api/emergency/my", JsonOptions)) ?? [];
    }
}
