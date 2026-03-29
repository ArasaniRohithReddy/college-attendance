using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class SystemConfigService : ISystemConfigService
{
    private readonly IRepository<SystemConfig> _configRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SystemConfigService(IRepository<SystemConfig> configRepo, IUnitOfWork unitOfWork)
    {
        _configRepo = configRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SystemConfigDto>> GetAllAsync(ConfigCategory? category = null)
    {
        var query = _configRepo.Query().AsQueryable();
        if (category.HasValue) query = query.Where(c => c.Category == category);

        return await query
            .OrderBy(c => c.Category).ThenBy(c => c.Key)
            .Select(c => new SystemConfigDto(c.Id, c.Key, c.Value, c.Description, c.DataType, c.Category))
            .ToListAsync();
    }

    public async Task<SystemConfigDto?> GetByKeyAsync(string key)
    {
        var config = await _configRepo.FirstOrDefaultAsync(c => c.Key == key);
        return config == null ? null : new SystemConfigDto(config.Id, config.Key, config.Value, config.Description, config.DataType, config.Category);
    }

    public async Task<T> GetValueAsync<T>(string key, T defaultValue)
    {
        var config = await _configRepo.FirstOrDefaultAsync(c => c.Key == key);
        if (config == null) return defaultValue;

        try
        {
            return (T)Convert.ChangeType(config.Value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    public async Task<SystemConfigDto> CreateAsync(Guid userId, CreateConfigRequest request)
    {
        var existing = await _configRepo.FirstOrDefaultAsync(c => c.Key == request.Key);
        if (existing != null)
            throw new InvalidOperationException($"Config key '{request.Key}' already exists");

        var config = new SystemConfig
        {
            Key = request.Key,
            Value = request.Value,
            Description = request.Description,
            DataType = request.DataType,
            Category = request.Category,
            ModifiedById = userId
        };

        await _configRepo.AddAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return new SystemConfigDto(config.Id, config.Key, config.Value, config.Description, config.DataType, config.Category);
    }

    public async Task<SystemConfigDto> UpdateAsync(string key, Guid userId, UpdateConfigRequest request)
    {
        var config = await _configRepo.FirstOrDefaultAsync(c => c.Key == key)
            ?? throw new KeyNotFoundException($"Config key '{key}' not found");

        config.Value = request.Value;
        config.ModifiedById = userId;
        config.UpdatedAt = DateTime.UtcNow;
        _configRepo.Update(config);
        await _unitOfWork.SaveChangesAsync();
        return new SystemConfigDto(config.Id, config.Key, config.Value, config.Description, config.DataType, config.Category);
    }

    public async Task DeleteAsync(string key)
    {
        var config = await _configRepo.FirstOrDefaultAsync(c => c.Key == key)
            ?? throw new KeyNotFoundException($"Config key '{key}' not found");

        _configRepo.Remove(config);
        await _unitOfWork.SaveChangesAsync();
    }
}
