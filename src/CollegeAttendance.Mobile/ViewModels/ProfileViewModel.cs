using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeAttendance.Mobile.Models;
using CollegeAttendance.Mobile.Services;

namespace CollegeAttendance.Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly AuthService _auth;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string? _studentId;

    [ObservableProperty]
    private string? _department;

    [ObservableProperty]
    private string? _profileImageUrl;

    public ProfileViewModel(AuthService auth)
    {
        _auth = auth;
    }

    [RelayCommand]
    private void LoadProfile()
    {
        var user = _auth.CurrentUser;
        if (user == null) return;

        FullName = user.FullName;
        Email = user.Email;
        Role = user.Role.ToString();
        StudentId = user.StudentId ?? user.EmployeeId;
        Department = user.DepartmentName;
        ProfileImageUrl = user.ProfileImageUrl;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
        if (!confirm) return;

        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
