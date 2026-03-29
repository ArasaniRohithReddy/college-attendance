using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class QRScanViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly LocationService _location;
    private readonly AuthService _auth;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isScanning = true;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private AttendanceDto? _result;

    public QRScanViewModel(ApiService api, LocationService location, AuthService auth)
    {
        _api = api;
        _location = location;
        _auth = auth;
    }

    [RelayCommand]
    private async Task ProcessQRCodeAsync(string qrToken)
    {
        if (IsBusy || string.IsNullOrWhiteSpace(qrToken)) return;
        IsBusy = true;
        IsScanning = false;
        StatusMessage = "Getting your location...";

        try
        {
            var loc = await _location.GetCurrentLocationAsync();
            if (loc == null)
            {
                StatusMessage = "Could not get location. Please enable GPS and try again.";
                IsSuccess = false;
                return;
            }

            StatusMessage = "Marking attendance...";
            var request = new MarkAttendanceRequest
            {
                QRToken = qrToken,
                Latitude = loc.Latitude,
                Longitude = loc.Longitude,
                DeviceId = DeviceInfo.Current.Idiom.ToString()
            };

            Result = await _api.MarkAttendanceAsync(request);
            IsSuccess = true;
            StatusMessage = $"Attendance marked: {Result.Status}";
        }
        catch (HttpRequestException ex)
        {
            IsSuccess = false;
            StatusMessage = $"Failed: {ex.Message}";
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ResetScanner()
    {
        IsScanning = true;
        IsSuccess = false;
        StatusMessage = null;
        Result = null;
    }
}
