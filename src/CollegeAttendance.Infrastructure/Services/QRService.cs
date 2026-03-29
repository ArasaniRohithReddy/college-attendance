using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QRCoder;

namespace CollegeAttendance.Infrastructure.Services;

public class QRService : IQRService
{
    private readonly IRepository<QRSession> _qrRepo;
    private readonly IRepository<ClassSession> _sessionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly byte[] _encryptionKey;

    public QRService(
        IRepository<QRSession> qrRepo,
        IRepository<ClassSession> sessionRepo,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _qrRepo = qrRepo;
        _sessionRepo = sessionRepo;
        _unitOfWork = unitOfWork;

        var keyString = configuration["QR:EncryptionKey"]
            ?? "CollegeAttendanceDefaultKey12345";
        _encryptionKey = Encoding.UTF8.GetBytes(keyString[..32]);
    }

    public async Task<QRSessionDto> GenerateQRAsync(Guid facultyId, GenerateQRRequest request)
    {
        var session = await _sessionRepo.GetByIdAsync(request.ClassSessionId)
            ?? throw new KeyNotFoundException("Class session not found");

        // Deactivate existing QR for this session
        var existing = await _qrRepo.FirstOrDefaultAsync(q =>
            q.ClassSessionId == request.ClassSessionId && q.IsActive);
        if (existing != null)
        {
            existing.IsActive = false;
            _qrRepo.Update(existing);
        }

        var expiresAt = DateTime.UtcNow.AddSeconds(request.ExpirationSeconds);
        var payload = GenerateEncryptedPayload(request.ClassSessionId, expiresAt);
        var qrToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var qrSession = new QRSession
        {
            ClassSessionId = request.ClassSessionId,
            GeneratedById = facultyId,
            EncryptedPayload = payload,
            QRToken = qrToken,
            ExpiresAt = expiresAt,
            IsActive = true
        };

        await _qrRepo.AddAsync(qrSession);
        await _unitOfWork.SaveChangesAsync();

        // Generate QR image
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(qrToken, QRCodeGenerator.ECCLevel.M);
        var qrCode = new PngByteQRCode(qrCodeData);
        var qrImageBytes = qrCode.GetGraphic(10);
        var qrImageBase64 = Convert.ToBase64String(qrImageBytes);

        return new QRSessionDto(
            qrSession.Id, qrToken, qrImageBase64,
            qrSession.GeneratedAt, qrSession.ExpiresAt,
            qrSession.IsActive, qrSession.ScanCount);
    }

    public async Task<QRSessionDto?> ValidateQRTokenAsync(string qrToken)
    {
        var qrSession = await _qrRepo.Query()
            .FirstOrDefaultAsync(q => q.QRToken == qrToken && q.IsActive);

        if (qrSession == null || qrSession.ExpiresAt < DateTime.UtcNow)
            return null;

        return new QRSessionDto(
            qrSession.Id, qrSession.QRToken, "",
            qrSession.GeneratedAt, qrSession.ExpiresAt,
            qrSession.IsActive, qrSession.ScanCount);
    }

    public async Task DeactivateQRAsync(Guid qrSessionId)
    {
        var qr = await _qrRepo.GetByIdAsync(qrSessionId)
            ?? throw new KeyNotFoundException("QR session not found");
        qr.IsActive = false;
        _qrRepo.Update(qr);
        await _unitOfWork.SaveChangesAsync();
    }

    public string GenerateEncryptedPayload(Guid classSessionId, DateTime expiresAt)
    {
        var payload = JsonSerializer.Serialize(new { classSessionId, expiresAt });
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(payload);
        var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

        return Convert.ToBase64String(result);
    }

    public bool ValidatePayload(string encryptedPayload, out Guid classSessionId, out DateTime expiresAt)
    {
        classSessionId = Guid.Empty;
        expiresAt = DateTime.MinValue;

        try
        {
            var data = Convert.FromBase64String(encryptedPayload);
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;

            var iv = new byte[16];
            Buffer.BlockCopy(data, 0, iv, 0, 16);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decrypted = decryptor.TransformFinalBlock(data, 16, data.Length - 16);
            var json = Encoding.UTF8.GetString(decrypted);

            var doc = JsonDocument.Parse(json);
            classSessionId = doc.RootElement.GetProperty("classSessionId").GetGuid();
            expiresAt = doc.RootElement.GetProperty("expiresAt").GetDateTime();

            return expiresAt > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    public byte[] GenerateQRCodeImage(string data)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
        var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(10);
    }
}
