using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;
using System.Collections.ObjectModel;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class GamificationViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _longestStreak;

    [ObservableProperty]
    private int _totalBadges;

    [ObservableProperty]
    private double _totalScore;

    [ObservableProperty]
    private int _currentRank;

    [ObservableProperty]
    private string _selectedPeriod = "weekly";

    public ObservableCollection<StudentBadgeDto> Badges { get; } = [];
    public ObservableCollection<LeaderboardEntryDto> Leaderboard { get; } = [];

    public GamificationViewModel(ApiService api)
    {
        _api = api;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var dashboard = await _api.GetGamificationDashboardAsync();

            CurrentStreak = dashboard.Streak.CurrentStreak;
            LongestStreak = dashboard.Streak.LongestStreak;
            TotalBadges = dashboard.TotalBadges;
            TotalScore = dashboard.TotalScore;
            CurrentRank = dashboard.CurrentRank;

            Badges.Clear();
            foreach (var b in dashboard.Badges)
                Badges.Add(b);

            await LoadLeaderboardAsync();
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
    private async Task LoadLeaderboardAsync()
    {
        try
        {
            var entries = await _api.GetLeaderboardAsync(SelectedPeriod);

            Leaderboard.Clear();
            foreach (var e in entries)
                Leaderboard.Add(e);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ChangePeriodAsync(string period)
    {
        SelectedPeriod = period;
        await LoadLeaderboardAsync();
    }
}
