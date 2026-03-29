using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class HostelService : IHostelService
{
    private readonly IRepository<Hostel> _hostelRepo;
    private readonly IRepository<HostelLog> _logRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public HostelService(IRepository<Hostel> hostelRepo, IRepository<HostelLog> logRepo,
        IRepository<User> userRepo, IUnitOfWork unitOfWork)
    {
        _hostelRepo = hostelRepo;
        _logRepo = logRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<HostelDto>> GetAllHostelsAsync() =>
        await _hostelRepo.Query()
            .Include(h => h.Warden)
            .Where(h => !h.IsDeleted)
            .Select(h => new HostelDto(h.Id, h.Name, h.Block, h.Capacity,
                h.Warden != null ? h.Warden.FullName : null,
                h.Residents.Count(r => !r.IsDeleted)))
            .ToListAsync();

    public async Task<HostelDto> CreateHostelAsync(CreateHostelRequest request)
    {
        var hostel = new Hostel
        {
            Name = request.Name, Block = request.Block,
            Capacity = request.Capacity, WardenId = request.WardenId
        };
        await _hostelRepo.AddAsync(hostel);
        await _unitOfWork.SaveChangesAsync();
        return new HostelDto(hostel.Id, hostel.Name, hostel.Block, hostel.Capacity, null, 0);
    }

    public async Task<HostelLogDto> LogEntryAsync(CreateHostelLogRequest request)
    {
        var log = new HostelLog
        {
            StudentId = request.StudentId,
            HostelId = request.HostelId,
            LogType = request.LogType,
            VerificationMethod = request.VerificationMethod,
            Timestamp = DateTime.UtcNow
        };
        await _logRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

        var student = await _userRepo.GetByIdAsync(request.StudentId);
        return new HostelLogDto(log.Id, log.StudentId, student?.FullName ?? "",
            log.LogType, log.Timestamp, log.VerificationMethod, null);
    }

    public async Task<List<HostelLogDto>> GetStudentLogsAsync(Guid studentId, DateTime from, DateTime to) =>
        await _logRepo.Query()
            .Include(l => l.Student)
            .Include(l => l.VerifiedBy)
            .Where(l => l.StudentId == studentId && l.Timestamp >= from && l.Timestamp <= to && !l.IsDeleted)
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new HostelLogDto(l.Id, l.StudentId, l.Student.FullName,
                l.LogType, l.Timestamp, l.VerificationMethod,
                l.VerifiedBy != null ? l.VerifiedBy.FullName : null))
            .ToListAsync();
}

public class MessService : IMessService
{
    private readonly IRepository<MessLog> _logRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public MessService(IRepository<MessLog> logRepo, IRepository<User> userRepo, IUnitOfWork unitOfWork)
    {
        _logRepo = logRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessLogDto> LogMealAsync(CreateMessLogRequest request)
    {
        var alreadyScanned = await _logRepo.AnyAsync(l =>
            l.StudentId == request.StudentId && l.MealType == request.MealType
            && l.Date == DateTime.UtcNow.Date && !l.IsDeleted);
        if (alreadyScanned)
            throw new InvalidOperationException("Meal already logged for this type today");

        var log = new MessLog
        {
            StudentId = request.StudentId,
            MealType = request.MealType,
            Date = DateTime.UtcNow.Date,
            VerificationMethod = request.VerificationMethod
        };
        await _logRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

        var student = await _userRepo.GetByIdAsync(request.StudentId);
        return new MessLogDto(log.Id, log.StudentId, student?.FullName ?? "",
            log.MealType, log.Date, log.ScannedAt, log.VerificationMethod);
    }

    public async Task<List<MessLogDto>> GetStudentLogsAsync(Guid studentId, DateTime from, DateTime to) =>
        await _logRepo.Query()
            .Include(l => l.Student)
            .Where(l => l.StudentId == studentId && l.Date >= from.Date && l.Date <= to.Date && !l.IsDeleted)
            .OrderByDescending(l => l.ScannedAt)
            .Select(l => new MessLogDto(l.Id, l.StudentId, l.Student.FullName,
                l.MealType, l.Date, l.ScannedAt, l.VerificationMethod))
            .ToListAsync();

    public async Task<MessAnalyticsDto> GetAnalyticsAsync(DateTime from, DateTime to)
    {
        var logs = await _logRepo.Query()
            .Where(l => l.Date >= from.Date && l.Date <= to.Date && !l.IsDeleted)
            .ToListAsync();

        return new MessAnalyticsDto(
            DateTime.UtcNow.Date,
            logs.Count(l => l.MealType == MealType.Breakfast),
            logs.Count(l => l.MealType == MealType.Lunch),
            logs.Count(l => l.MealType == MealType.Snacks),
            logs.Count(l => l.MealType == MealType.Dinner));
    }
}
