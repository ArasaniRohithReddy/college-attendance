using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;
using System.Collections.ObjectModel;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _greeting = "Welcome";

    [ObservableProperty]
    private UserRole _userRole;

    [ObservableProperty]
    private int _todaySessionCount;

    [ObservableProperty]
    private int _unreadNotifications;

    [ObservableProperty]
    private double _attendancePercentage;

    [ObservableProperty]
    private int _totalStudents;

    [ObservableProperty]
    private int _defaulterCount;

    public ObservableCollection<ClassSessionDto> TodaySessions { get; } = [];

    public DashboardViewModel(ApiService api, AuthService auth)
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

            Greeting = $"Hello, {user.FullName.Split(' ')[0]}!";
            UserRole = user.Role;

            var sessionsTask = _api.GetTodaySessionsAsync();
            var notifTask = _api.GetUnreadCountAsync();

            await Task.WhenAll(sessionsTask, notifTask);

            TodaySessions.Clear();
            foreach (var s in await sessionsTask)
                TodaySessions.Add(s);
            TodaySessionCount = TodaySessions.Count;
            UnreadNotifications = await notifTask;

            if (user.Role is UserRole.Admin or UserRole.Faculty)
            {
                try
                {
                    var dashboard = await _api.GetDashboardAsync();
                    AttendancePercentage = dashboard.OverallAttendancePercentage;
                    TotalStudents = dashboard.TotalStudents;
                    DefaulterCount = dashboard.DefaulterCount;
                }
                catch { /* Analytics may not be available for all roles */ }
            }
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
    private async Task NavigateToSessionAsync(ClassSessionDto session)
    {
        await Shell.Current.GoToAsync($"session?id={session.Id}");
    }

    [RelayCommand]
    private async Task NavigateToScanAsync()
    {
        await Shell.Current.GoToAsync("scan");
    }
}
