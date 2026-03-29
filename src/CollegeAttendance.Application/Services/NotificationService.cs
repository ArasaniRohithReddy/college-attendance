using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notifRepo;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IRepository<Notification> notifRepo, IUnitOfWork unitOfWork)
    {
        _notifRepo = notifRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        var query = _notifRepo.Query()
            .Where(n => n.UserId == userId && !n.IsDeleted);

        if (unreadOnly) query = query.Where(n => !n.IsRead);

        return await query.OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAt, n.ActionUrl))
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notif = await _notifRepo.GetByIdAsync(notificationId)
            ?? throw new KeyNotFoundException("Notification not found");
        notif.IsRead = true;
        notif.ReadAt = DateTime.UtcNow;
        _notifRepo.Update(notif);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _notifRepo.FindAsync(n => n.UserId == userId && !n.IsRead);
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            _notifRepo.Update(n);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendNotificationAsync(Guid userId, string title, string message, NotificationType type, string? actionUrl = null)
    {
        await _notifRepo.AddAsync(new Notification
        {
            UserId = userId, Title = title, Message = message, Type = type, ActionUrl = actionUrl
        });
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendBulkNotificationAsync(List<Guid> userIds, string title, string message, NotificationType type)
    {
        var notifications = userIds.Select(uid => new Notification
        {
            UserId = uid, Title = title, Message = message, Type = type
        });
        await _notifRepo.AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId) =>
        await _notifRepo.CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
}
