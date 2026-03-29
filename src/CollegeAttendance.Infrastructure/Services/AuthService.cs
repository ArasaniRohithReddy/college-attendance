using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CollegeAttendance.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IRepository<User> userRepo, IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthResponse> GoogleLoginAsync(string idToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _configuration["Google:ClientId"] }
        });

        var user = await _userRepo.Query()
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user == null)
        {
            user = new User
            {
                Email = payload.Email,
                FullName = payload.Name,
                GoogleId = payload.Subject,
                ProfileImageUrl = payload.Picture,
                Role = UserRole.Student, // Default role
                IsActive = true
            };
            await _userRepo.AddAsync(user);
        }
        else
        {
            user.GoogleId = payload.Subject;
            user.ProfileImageUrl = payload.Picture;
            user.LastLoginAt = DateTime.UtcNow;
            _userRepo.Update(user);
        }

        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _unitOfWork.SaveChangesAsync();

        var userDto = new UserDto(
            user.Id, user.Email, user.FullName, user.Role,
            user.StudentId, user.EmployeeId, user.ProfileImageUrl,
            user.Phone, user.DepartmentId, user.Department?.Name, user.IsActive);

        var accessToken = GenerateJwtToken(userDto);
        var expiresAt = DateTime.UtcNow.AddHours(
            double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24"));

        return new AuthResponse(accessToken, refreshToken, expiresAt, userDto);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepo.Query()
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired");

        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();

        var userDto = new UserDto(
            user.Id, user.Email, user.FullName, user.Role,
            user.StudentId, user.EmployeeId, user.ProfileImageUrl,
            user.Phone, user.DepartmentId, user.Department?.Name, user.IsActive);

        var accessToken = GenerateJwtToken(userDto);
        var expiresAt = DateTime.UtcNow.AddHours(
            double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24"));

        return new AuthResponse(accessToken, newRefreshToken, expiresAt, userDto);
    }

    public async Task RevokeTokenAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public string GenerateJwtToken(UserDto user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key not configured")));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("department_id", user.DepartmentId?.ToString() ?? "")
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
