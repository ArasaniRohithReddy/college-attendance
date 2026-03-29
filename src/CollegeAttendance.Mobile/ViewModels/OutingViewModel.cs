using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;
using System.Collections.ObjectModel;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class OutingViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isStudent;

    // New outing form fields
    [ObservableProperty]
    private string _purpose = string.Empty;

    [ObservableProperty]
    private string _destination = string.Empty;

    [ObservableProperty]
    private DateTime _outDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan _outTime = new(9, 0, 0);

    [ObservableProperty]
    private DateTime _returnDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan _returnTime = new(18, 0, 0);

    public ObservableCollection<OutingRequestDto> Outings { get; } = [];

    public OutingViewModel(ApiService api, AuthService auth)
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
            IsStudent = _auth.CurrentUser?.Role == UserRole.Student;

            var outings = IsStudent
                ? await _api.GetMyOutingsAsync()
                : await _api.GetAllOutingsAsync();

            Outings.Clear();
            foreach (var o in outings.OrderByDescending(o => o.RequestedOutTime))
                Outings.Add(o);
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
    private async Task SubmitRequestAsync()
    {
        if (string.IsNullOrWhiteSpace(Purpose) || string.IsNullOrWhiteSpace(Destination))
        {
            await Shell.Current.DisplayAlert("Validation", "Please fill in all fields.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var request = new CreateOutingRequest
            {
                Purpose = Purpose,
                Destination = Destination,
                RequestedOutTime = OutDate.Date + OutTime,
                ExpectedReturnTime = ReturnDate.Date + ReturnTime
            };

            await _api.CreateOutingAsync(request);
            Purpose = string.Empty;
            Destination = string.Empty;
            await Shell.Current.DisplayAlert("Success", "Outing request submitted.", "OK");
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
    private async Task ApproveAsync(OutingRequestDto outing)
    {
        IsBusy = true;
        try
        {
            await _api.ApproveOutingAsync(outing.Id);
            await Shell.Current.DisplayAlert("Success", "Request approved.", "OK");
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
    private async Task RejectAsync(OutingRequestDto outing)
    {
        var remarks = await Shell.Current.DisplayPromptAsync("Reject", "Enter rejection reason:");
        if (string.IsNullOrEmpty(remarks)) return;

        IsBusy = true;
        try
        {
            await _api.RejectOutingAsync(outing.Id, remarks);
            await Shell.Current.DisplayAlert("Success", "Request rejected.", "OK");
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
