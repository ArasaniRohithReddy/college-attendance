using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;
using System.Collections.ObjectModel;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class LeaveViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _showForm;

    // Form fields
    [ObservableProperty]
    private LeaveType _selectedLeaveType;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private string _reason = string.Empty;

    public ObservableCollection<LeaveRequestDto> Leaves { get; } = [];

    public List<LeaveType> LeaveTypes { get; } =
        [LeaveType.Medical, LeaveType.Personal, LeaveType.Family, LeaveType.Academic, LeaveType.Other];

    public LeaveViewModel(ApiService api)
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
            var result = await _api.GetMyLeavesAsync();
            Leaves.Clear();
            foreach (var l in result.Items.OrderByDescending(l => l.StartDate))
                Leaves.Add(l);
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
    private void ToggleForm()
    {
        ShowForm = !ShowForm;
    }

    [RelayCommand]
    private async Task SubmitLeaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Reason))
        {
            await Shell.Current.DisplayAlert("Validation", "Please enter a reason.", "OK");
            return;
        }

        if (EndDate <= StartDate)
        {
            await Shell.Current.DisplayAlert("Validation", "End date must be after start date.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var request = new CreateLeaveRequest
            {
                LeaveType = SelectedLeaveType,
                StartDate = StartDate,
                EndDate = EndDate,
                Reason = Reason
            };

            await _api.CreateLeaveAsync(request);
            Reason = string.Empty;
            ShowForm = false;
            await Shell.Current.DisplayAlert("Success", "Leave request submitted.", "OK");
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

    [RelayCommand]
    private async Task CancelLeaveAsync(LeaveRequestDto leave)
    {
        if (leave.Status != LeaveStatus.Pending)
        {
            await Shell.Current.DisplayAlert("Info", "Only pending leaves can be cancelled.", "OK");
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert("Cancel Leave", "Cancel this leave request?", "Yes", "No");
        if (!confirm) return;

        IsBusy = true;
        try
        {
            await _api.CancelLeaveAsync(leave.Id);
            await Shell.Current.DisplayAlert("Success", "Leave cancelled.", "OK");
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
