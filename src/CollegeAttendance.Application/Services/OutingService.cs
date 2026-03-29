using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class OutingService : IOutingService
{
    private readonly IRepository<OutingRequest> _outingRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IQRService _qrService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public OutingService(
        IRepository<OutingRequest> outingRepo,
        IRepository<User> userRepo,
        IQRService qrService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _outingRepo = outingRepo;
        _userRepo = userRepo;
        _qrService = qrService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<OutingRequestDto> CreateRequestAsync(Guid studentId, CreateOutingRequest request)
    {
        var outing = new OutingRequest
        {
            StudentId = studentId,
            Purpose = request.Purpose,
            Destination = request.Destination,
            RequestedOutTime = request.RequestedOutTime,
            ExpectedReturnTime = request.ExpectedReturnTime,
            EmergencyContact = request.EmergencyContact
        };

        await _outingRepo.AddAsync(outing);
        await _unitOfWork.SaveChangesAsync();

        var student = await _userRepo.GetByIdAsync(studentId);
        var wardens = await _userRepo.FindAsync(u => u.Role == UserRole.Warden && !u.IsDeleted);
        foreach (var warden in wardens)
        {
            await _notificationService.SendNotificationAsync(
                warden.Id, "New Outing Request",
                $"{student?.FullName} has requested an outing to {request.Destination}",
                NotificationType.OutingApproval);
        }

        return MapToDto(outing, student?.FullName ?? "");
    }

    public async Task<List<OutingRequestDto>> GetStudentRequestsAsync(Guid studentId) =>
        await _outingRepo.Query()
            .Include(o => o.Student)
            .Where(o => o.StudentId == studentId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => MapToDto(o, o.Student.FullName))
            .ToListAsync();

    public async Task<List<OutingRequestDto>> GetPendingRequestsAsync(string role)
    {
        var status = role == "Security" ? OutingStatus.ApprovedByWarden : OutingStatus.Pending;

        return await _outingRepo.Query()
            .Include(o => o.Student)
            .Where(o => o.Status == status && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => MapToDto(o, o.Student.FullName))
            .ToListAsync();
    }

    public async Task WardenApproveAsync(Guid requestId, Guid wardenId)
    {
        var outing = await _outingRepo.Query()
            .Include(o => o.Student)
            .FirstOrDefaultAsync(o => o.Id == requestId)
            ?? throw new KeyNotFoundException("Outing request not found");

        if (outing.Status != OutingStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved");

        outing.Status = OutingStatus.ApprovedByWarden;
        outing.ApprovedByWardenId = wardenId;
        outing.GatePassQRCode = Guid.NewGuid().ToString("N");
        outing.GatePassExpiresAt = outing.ExpectedReturnTime.AddHours(2);
        outing.UpdatedAt = DateTime.UtcNow;

        _outingRepo.Update(outing);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            outing.StudentId, "Outing Approved",
            "Your outing request has been approved. Show your gate pass at security.",
            NotificationType.OutingApproval);
    }

    public async Task WardenRejectAsync(Guid requestId, Guid wardenId, string reason)
    {
        var outing = await _outingRepo.Query()
            .Include(o => o.Student)
            .FirstOrDefaultAsync(o => o.Id == requestId)
            ?? throw new KeyNotFoundException("Outing request not found");

        if (outing.Status != OutingStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected");

        outing.Status = OutingStatus.Rejected;
        outing.WardenRemarks = reason;
        outing.UpdatedAt = DateTime.UtcNow;
        _outingRepo.Update(outing);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            outing.StudentId, "Outing Rejected",
            $"Your outing request was rejected. Reason: {reason}",
            NotificationType.OutingRejection);
    }

    public async Task SecurityCheckoutAsync(Guid requestId, Guid securityId)
    {
        var outing = await _outingRepo.GetByIdAsync(requestId)
            ?? throw new KeyNotFoundException("Outing request not found");

        if (outing.Status != OutingStatus.ApprovedByWarden)
            throw new InvalidOperationException("Outing must be approved by warden first");

        outing.Status = OutingStatus.CheckedOut;
        outing.ActualOutTime = DateTime.UtcNow;
        outing.ProcessedBySecurityId = securityId;
        outing.UpdatedAt = DateTime.UtcNow;
        _outingRepo.Update(outing);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SecurityCheckinAsync(Guid requestId, Guid securityId)
    {
        var outing = await _outingRepo.GetByIdAsync(requestId)
            ?? throw new KeyNotFoundException("Outing request not found");

        if (outing.Status != OutingStatus.CheckedOut)
            throw new InvalidOperationException("Student hasn't checked out yet");

        outing.Status = OutingStatus.CheckedIn;
        outing.ActualReturnTime = DateTime.UtcNow;
        outing.UpdatedAt = DateTime.UtcNow;
        _outingRepo.Update(outing);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<byte[]> GenerateGatePassQRAsync(Guid requestId)
    {
        var outing = await _outingRepo.Query()
            .Include(o => o.Student)
            .FirstOrDefaultAsync(o => o.Id == requestId)
            ?? throw new KeyNotFoundException("Outing request not found");

        if (outing.Status != OutingStatus.ApprovedByWarden && outing.Status != OutingStatus.CheckedOut)
            throw new InvalidOperationException("Gate pass only available for approved outings");

        var payload = $"{outing.Id}|{outing.StudentId}|{outing.GatePassQRCode}|{outing.GatePassExpiresAt:O}";
        return _qrService.GenerateQRCodeImage(payload);
    }

    private static OutingRequestDto MapToDto(OutingRequest o, string studentName) => new(
        o.Id, o.StudentId, studentName, o.Purpose, o.Destination,
        o.RequestedOutTime, o.ExpectedReturnTime, o.ActualOutTime, o.ActualReturnTime,
        o.Status, o.WardenRemarks, o.SecurityRemarks,
        o.GatePassQRCode, o.GatePassExpiresAt);
}
