using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Application.Interfaces;

public interface ISystemConfigService
{
    Task<List<SystemConfigDto>> GetAllAsync(ConfigCategory? category = null);
    Task<SystemConfigDto?> GetByKeyAsync(string key);
    Task<T> GetValueAsync<T>(string key, T defaultValue);
    Task<SystemConfigDto> CreateAsync(Guid userId, CreateConfigRequest request);
    Task<SystemConfigDto> UpdateAsync(string key, Guid userId, UpdateConfigRequest request);
    Task DeleteAsync(string key);
}
