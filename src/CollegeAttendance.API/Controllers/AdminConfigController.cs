using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/admin/config")]
[Authorize(Roles = "Admin")]
public class AdminConfigController : ControllerBase
{
    private readonly ISystemConfigService _configService;

    public AdminConfigController(ISystemConfigService configService) => _configService = configService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<SystemConfigDto>>> GetAll([FromQuery] ConfigCategory? category)
        => Ok(await _configService.GetAllAsync(category));

    [HttpGet("{key}")]
    public async Task<ActionResult<SystemConfigDto>> GetByKey(string key)
    {
        var config = await _configService.GetByKeyAsync(key);
        return config is null ? NotFound() : Ok(config);
    }

    [HttpPost]
    public async Task<ActionResult<SystemConfigDto>> Create([FromBody] CreateConfigRequest request)
        => Ok(await _configService.CreateAsync(UserId, request));

    [HttpPut("{key}")]
    public async Task<ActionResult<SystemConfigDto>> Update(string key, [FromBody] UpdateConfigRequest request)
        => Ok(await _configService.UpdateAsync(key, UserId, request));

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        await _configService.DeleteAsync(key);
        return NoContent();
    }
}
