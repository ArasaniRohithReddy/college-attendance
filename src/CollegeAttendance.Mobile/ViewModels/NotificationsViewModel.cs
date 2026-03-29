using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;
using System.Collections.ObjectModel;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<NotificationDto> Notifications { get; } = [];

    public NotificationsViewModel(ApiService api)
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
            var notifs = await _api.GetNotificationsAsync();
            Notifications.Clear();
            foreach (var n in notifs.OrderByDescending(n => n.CreatedAt))
                Notifications.Add(n);
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
    private async Task MarkAsReadAsync(NotificationDto notification)
    {
        if (notification.IsRead) return;
        try
        {
            await _api.MarkNotificationReadAsync(notification.Id);
            notification.IsRead = true;
            // Refresh to update UI
            await LoadDataAsync();
        }
        catch { /* Silently fail for read status */ }
    }

    [RelayCommand]
    private async Task MarkAllReadAsync()
    {
        IsBusy = true;
        try
        {
            await _api.MarkAllNotificationsReadAsync();
            await LoadDataAsync();
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
