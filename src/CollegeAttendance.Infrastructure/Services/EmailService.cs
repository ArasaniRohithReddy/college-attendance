using CollegeAttendance.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace CollegeAttendance.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUser = _configuration["Smtp:User"] ?? "";
        var smtpPass = _configuration["Smtp:Password"] ?? "";
        var fromEmail = _configuration["Smtp:From"] ?? smtpUser;

        if (string.IsNullOrEmpty(smtpUser)) return; // Skip if SMTP not configured

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var message = new MailMessage(fromEmail, to, subject, htmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(message);
    }

    public async Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string htmlBody)
    {
        foreach (var recipient in recipients)
        {
            try
            {
                await SendEmailAsync(recipient, subject, htmlBody);
            }
            catch
            {
                // Log and continue with next recipient
            }
        }
    }

    public async Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> templateData)
    {
        var (subject, body) = templateName switch
        {
            "curfew_violation" => (
                "Hostel Curfew Violation Alert",
                $"<h2>Curfew Violation Notice</h2><p>Dear Parent,</p><p>Your ward <strong>{templateData.GetValueOrDefault("studentName", "")}</strong> has violated the hostel curfew at <strong>{templateData.GetValueOrDefault("time", "")}</strong>.</p><p>They were <strong>{templateData.GetValueOrDefault("minutesLate", "0")} minutes</strong> late returning to the hostel.</p><p>Best regards,<br/>College Administration</p>"),

            "outing_approved" => (
                "Outing Request Approved",
                $"<h2>Outing Approved</h2><p>Dear Parent,</p><p>Your ward <strong>{templateData.GetValueOrDefault("studentName", "")}</strong> has been approved for an outing to <strong>{templateData.GetValueOrDefault("destination", "")}</strong> on <strong>{templateData.GetValueOrDefault("date", "")}</strong>.</p>"),

            "sos_alert" => (
                "⚠️ Emergency SOS Alert",
                $"<h2 style='color:red'>EMERGENCY SOS ALERT</h2><p>Your ward <strong>{templateData.GetValueOrDefault("studentName", "")}</strong> has triggered an emergency SOS alert.</p><p>Location: {templateData.GetValueOrDefault("location", "Unknown")}</p><p>Message: {templateData.GetValueOrDefault("message", "N/A")}</p><p>Please contact the college administration immediately.</p>"),

            "low_attendance" => (
                "Low Attendance Warning",
                $"<h2>Attendance Warning</h2><p>Dear Parent,</p><p>Your ward <strong>{templateData.GetValueOrDefault("studentName", "")}</strong> has an attendance of <strong>{templateData.GetValueOrDefault("percentage", "0")}%</strong> which is below the required threshold.</p><p>Please ensure regular attendance to avoid academic consequences.</p>"),

            _ => ("College Notification", $"<p>{string.Join("<br/>", templateData.Select(kv => $"{kv.Key}: {kv.Value}"))}</p>")
        };

        await SendEmailAsync(to, subject, body);
    }
}
