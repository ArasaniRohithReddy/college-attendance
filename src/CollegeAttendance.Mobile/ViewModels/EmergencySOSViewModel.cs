using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;
using System.Collections.ObjectModel;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class EmergencySOSViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly LocationService _location;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isSending;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private SOSPriority _selectedPriority = SOSPriority.High;

    [ObservableProperty]
    private bool _hasActiveAlert;

    public ObservableCollection<EmergencySOSDto> MyAlerts { get; } = [];

    public List<SOSPriority> Priorities { get; } =
        [SOSPriority.Low, SOSPriority.Medium, SOSPriority.High, SOSPriority.Critical];

    public EmergencySOSViewModel(ApiService api, LocationService location)
    {
        _api = api;
        _location = location;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var alerts = await _api.GetMySOSAsync();
            MyAlerts.Clear();
            foreach (var a in alerts.OrderByDescending(a => a.CreatedAt))
                MyAlerts.Add(a);

            HasActiveAlert = alerts.Any(a => a.Status is SOSStatus.Active or SOSStatus.Acknowledged or SOSStatus.Responding);
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
    private async Task SendSOSAsync()
    {
        if (HasActiveAlert)
        {
            await Shell.Current.DisplayAlert("Active Alert", "You already have an active SOS alert.", "OK");
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert(
            "Emergency SOS",
            "This will send an emergency alert to campus security. Continue?",
            "Send SOS", "Cancel");

        if (!confirm) return;

        IsSending = true;
        try
        {
            var loc = await _location.GetCurrentLocationAsync();
            double lat = loc?.Latitude ?? 0;
            double lng = loc?.Longitude ?? 0;

            var request = new CreateSOSRequest
            {
                Latitude = lat,
                Longitude = lng,
                Message = string.IsNullOrWhiteSpace(Message) ? null : Message,
                Priority = SelectedPriority
            };

            await _api.CreateSOSAsync(request);
            Message = string.Empty;
            await Shell.Current.DisplayAlert("SOS Sent", "Emergency alert sent. Help is on the way.", "OK");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to send SOS: {ex.Message}", "OK");
        }
        finally
        {
            IsSending = false;
        }
    }
}
