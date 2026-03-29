using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;

namespace CollegeAttendance.Mobile.ViewModels;

[QueryProperty(nameof(SessionId), "id")]
public partial class SessionDetailViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    private string? _sessionId;

    [ObservableProperty]
    private ClassSessionDto? _session;

    [ObservableProperty]
    private QRSessionDto? _activeQR;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isFaculty;

    [ObservableProperty]
    private bool _isQRActive;

    public SessionDetailViewModel(ApiService api, AuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    partial void OnSessionIdChanged(string? value)
    {
        if (Guid.TryParse(value, out _))
            LoadSessionCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadSessionAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(SessionId)) return;
        IsBusy = true;

        try
        {
            Session = await _api.GetSessionAsync(Guid.Parse(SessionId));
            IsFaculty = _auth.CurrentUser?.Role is UserRole.Faculty or UserRole.Admin;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GenerateQRAsync()
    {
        if (Session == null) return;
        IsBusy = true;

        try
        {
            ActiveQR = await _api.GenerateQRAsync(Session.Id);
            IsQRActive = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
