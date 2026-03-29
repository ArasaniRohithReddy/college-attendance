using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;
using System.Collections.ObjectModel;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class AttendanceViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private double _overallPercentage;

    public ObservableCollection<AttendanceDto> Records { get; } = [];

    public AttendanceViewModel(ApiService api, AuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var user = _auth.CurrentUser;
            if (user == null) return;

            var records = await _api.GetMyAttendanceAsync(user.Id);
            Records.Clear();
            foreach (var r in records.OrderByDescending(r => r.MarkedAt))
                Records.Add(r);

            var total = records.Count;
            var present = records.Count(r => r.Status is AttendanceStatus.Present or AttendanceStatus.Late);
            OverallPercentage = total > 0 ? Math.Round(present * 100.0 / total, 1) : 0;
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
