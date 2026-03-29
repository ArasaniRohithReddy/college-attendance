using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface IBulkImportService
{
    Task<BulkImportResultDto> ImportStudentsAsync(Stream fileStream, Guid importedById);
    Task<BulkImportResultDto> ImportCoursesAsync(Stream fileStream, Guid importedById);
    Task<byte[]> GenerateStudentTemplateAsync();
    Task<byte[]> GenerateCourseTemplateAsync();
}
