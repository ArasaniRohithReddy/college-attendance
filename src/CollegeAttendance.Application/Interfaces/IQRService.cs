using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface IQRService
{
    Task<QRSessionDto> GenerateQRAsync(Guid facultyId, GenerateQRRequest request);
    Task<QRSessionDto?> ValidateQRTokenAsync(string qrToken);
    Task DeactivateQRAsync(Guid qrSessionId);
    string GenerateEncryptedPayload(Guid classSessionId, DateTime expiresAt);
    bool ValidatePayload(string encryptedPayload, out Guid classSessionId, out DateTime expiresAt);
    byte[] GenerateQRCodeImage(string data);
}
