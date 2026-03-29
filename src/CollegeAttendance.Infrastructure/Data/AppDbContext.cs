using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<QRSession> QRSessions => Set<QRSession>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Hostel> Hostels => Set<Hostel>();
    public DbSet<HostelLog> HostelLogs => Set<HostelLog>();
    public DbSet<MessLog> MessLogs => Set<MessLog>();
    public DbSet<OutingRequest> OutingRequests => Set<OutingRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Campus Platform entities
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<StudentBadge> StudentBadges => Set<StudentBadge>();
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();
    public DbSet<AttendanceStreak> AttendanceStreaks => Set<AttendanceStreak>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<EmergencySOS> EmergencySOSAlerts => Set<EmergencySOS>();
    public DbSet<OfflineSyncLog> OfflineSyncLogs => Set<OfflineSyncLog>();
    public DbSet<DeviceBinding> DeviceBindings => Set<DeviceBinding>();
    public DbSet<FraudLog> FraudLogs => Set<FraudLog>();
    public DbSet<ScheduledNotification> ScheduledNotifications => Set<ScheduledNotification>();
    public DbSet<CurfewLog> CurfewLogs => Set<CurfewLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.GoogleId);
            entity.HasIndex(u => u.StudentId);
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.FullName).HasMaxLength(200);
            entity.Property(u => u.Phone).HasMaxLength(20);
            entity.HasQueryFilter(u => !u.IsDeleted);

            entity.HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(u => u.Hostel)
                .WithMany(h => h.Residents)
                .HasForeignKey(u => u.HostelId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Department
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(d => d.Code).IsUnique();
            entity.Property(d => d.Name).HasMaxLength(200);
            entity.Property(d => d.Code).HasMaxLength(20);
            entity.HasQueryFilter(d => !d.IsDeleted);
        });

        // Course
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(c => c.Code).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(200);
            entity.Property(c => c.Code).HasMaxLength(20);
            entity.HasQueryFilter(c => !c.IsDeleted);

            entity.HasOne(c => c.Department)
                .WithMany(d => d.Courses)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Faculty)
                .WithMany()
                .HasForeignKey(c => c.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CourseEnrollment
        modelBuilder.Entity<CourseEnrollment>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();

            entity.HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ClassSession
        modelBuilder.Entity<ClassSession>(entity =>
        {
            entity.HasIndex(s => new { s.CourseId, s.ScheduledDate });
            entity.HasIndex(s => s.FacultyId);
            entity.Property(s => s.Title).HasMaxLength(200);
            entity.Property(s => s.Room).HasMaxLength(50);
            entity.HasQueryFilter(s => !s.IsDeleted);

            entity.HasOne(s => s.Course)
                .WithMany(c => c.Sessions)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Faculty)
                .WithMany()
                .HasForeignKey(s => s.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // QRSession
        modelBuilder.Entity<QRSession>(entity =>
        {
            entity.HasIndex(q => q.QRToken).IsUnique();
            entity.HasIndex(q => q.ClassSessionId).IsUnique();

            entity.HasOne(q => q.ClassSession)
                .WithOne(s => s.QRSession)
                .HasForeignKey<QRSession>(q => q.ClassSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(q => q.GeneratedBy)
                .WithMany()
                .HasForeignKey(q => q.GeneratedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AttendanceRecord
        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasIndex(a => new { a.StudentId, a.ClassSessionId }).IsUnique();
            entity.HasIndex(a => a.MarkedAt);
            entity.HasQueryFilter(a => !a.IsDeleted);

            entity.HasOne(a => a.Student)
                .WithMany(u => u.AttendanceRecords)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.ClassSession)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(a => a.ClassSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.QRSession)
                .WithMany()
                .HasForeignKey(a => a.QRSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.MarkedBy)
                .WithMany()
                .HasForeignKey(a => a.MarkedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Hostel
        modelBuilder.Entity<Hostel>(entity =>
        {
            entity.Property(h => h.Name).HasMaxLength(200);
            entity.HasQueryFilter(h => !h.IsDeleted);

            entity.HasOne(h => h.Warden)
                .WithMany()
                .HasForeignKey(h => h.WardenId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // HostelLog
        modelBuilder.Entity<HostelLog>(entity =>
        {
            entity.HasIndex(l => new { l.StudentId, l.Timestamp });
            entity.HasQueryFilter(l => !l.IsDeleted);

            entity.HasOne(l => l.Student)
                .WithMany()
                .HasForeignKey(l => l.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Hostel)
                .WithMany(h => h.Logs)
                .HasForeignKey(l => l.HostelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.VerifiedBy)
                .WithMany()
                .HasForeignKey(l => l.VerifiedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // MessLog
        modelBuilder.Entity<MessLog>(entity =>
        {
            entity.HasIndex(m => new { m.StudentId, m.MealType, m.Date }).IsUnique();
            entity.HasQueryFilter(m => !m.IsDeleted);

            entity.HasOne(m => m.Student)
                .WithMany()
                .HasForeignKey(m => m.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OutingRequest
        modelBuilder.Entity<OutingRequest>(entity =>
        {
            entity.HasIndex(o => o.GatePassQRCode);
            entity.HasIndex(o => new { o.StudentId, o.Status });
            entity.Property(o => o.Purpose).HasMaxLength(500);
            entity.Property(o => o.Destination).HasMaxLength(500);
            entity.HasQueryFilter(o => !o.IsDeleted);

            entity.HasOne(o => o.Student)
                .WithMany(u => u.OutingRequests)
                .HasForeignKey(o => o.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.ApprovedByWarden)
                .WithMany()
                .HasForeignKey(o => o.ApprovedByWardenId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(o => o.ProcessedBySecurity)
                .WithMany()
                .HasForeignKey(o => o.ProcessedBySecurityId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(n => new { n.UserId, n.IsRead });
            entity.Property(n => n.Title).HasMaxLength(200);
            entity.Property(n => n.Message).HasMaxLength(1000);
            entity.HasQueryFilter(n => !n.IsDeleted);

            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => a.EntityName);
            entity.HasIndex(a => a.UserId);
            entity.Property(a => a.EntityName).HasMaxLength(100);
            entity.Property(a => a.Action).HasMaxLength(50);

            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== Campus Platform Entities =====

        // Badge
        modelBuilder.Entity<Badge>(entity =>
        {
            entity.Property(b => b.Name).HasMaxLength(100);
            entity.Property(b => b.Description).HasMaxLength(500);
            entity.HasQueryFilter(b => !b.IsDeleted);
        });

        // StudentBadge
        modelBuilder.Entity<StudentBadge>(entity =>
        {
            entity.HasIndex(sb => new { sb.StudentId, sb.BadgeId }).IsUnique();

            entity.HasOne(sb => sb.Student)
                .WithMany()
                .HasForeignKey(sb => sb.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sb => sb.Badge)
                .WithMany(b => b.StudentBadges)
                .HasForeignKey(sb => sb.BadgeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaderboardEntry
        modelBuilder.Entity<LeaderboardEntry>(entity =>
        {
            entity.HasIndex(le => new { le.Period, le.Rank });
            entity.HasIndex(le => new { le.StudentId, le.Period }).IsUnique();
            entity.Property(le => le.Period).HasMaxLength(20);

            entity.HasOne(le => le.Student)
                .WithMany()
                .HasForeignKey(le => le.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(le => le.Department)
                .WithMany()
                .HasForeignKey(le => le.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // AttendanceStreak
        modelBuilder.Entity<AttendanceStreak>(entity =>
        {
            entity.HasIndex(s => s.StudentId).IsUnique();

            entity.HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SystemConfig
        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasIndex(sc => sc.Key).IsUnique();
            entity.Property(sc => sc.Key).HasMaxLength(100);
            entity.Property(sc => sc.Value).HasMaxLength(2000);
            entity.Property(sc => sc.DataType).HasMaxLength(20);

            entity.HasOne(sc => sc.ModifiedBy)
                .WithMany()
                .HasForeignKey(sc => sc.ModifiedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // LeaveRequest
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasIndex(lr => new { lr.StudentId, lr.Status });
            entity.Property(lr => lr.Reason).HasMaxLength(1000);
            entity.HasQueryFilter(lr => !lr.IsDeleted);

            entity.HasOne(lr => lr.Student)
                .WithMany()
                .HasForeignKey(lr => lr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lr => lr.ApprovedBy)
                .WithMany()
                .HasForeignKey(lr => lr.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(lr => lr.Course)
                .WithMany()
                .HasForeignKey(lr => lr.CourseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // EmergencySOS
        modelBuilder.Entity<EmergencySOS>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RespondedBy)
                .WithMany()
                .HasForeignKey(e => e.RespondedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // OfflineSyncLog
        modelBuilder.Entity<OfflineSyncLog>(entity =>
        {
            entity.HasIndex(o => new { o.UserId, o.SyncStatus });
            entity.Property(o => o.Action).HasMaxLength(50);
            entity.Property(o => o.DeviceId).HasMaxLength(200);

            entity.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DeviceBinding
        modelBuilder.Entity<DeviceBinding>(entity =>
        {
            entity.HasIndex(db => new { db.UserId, db.DeviceFingerprint }).IsUnique();
            entity.Property(db => db.DeviceFingerprint).HasMaxLength(500);
            entity.Property(db => db.DeviceName).HasMaxLength(200);
            entity.Property(db => db.Platform).HasMaxLength(50);
            entity.Property(db => db.IpAddress).HasMaxLength(50);

            entity.HasOne(db => db.User)
                .WithMany()
                .HasForeignKey(db => db.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // FraudLog
        modelBuilder.Entity<FraudLog>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.FraudType });
            entity.Property(f => f.Description).HasMaxLength(1000);
            entity.HasQueryFilter(f => !f.IsDeleted);

            entity.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.ClassSession)
                .WithMany()
                .HasForeignKey(f => f.ClassSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(f => f.ResolvedBy)
                .WithMany()
                .HasForeignKey(f => f.ResolvedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ScheduledNotification
        modelBuilder.Entity<ScheduledNotification>(entity =>
        {
            entity.HasIndex(sn => new { sn.ScheduledAt, sn.IsSent });
            entity.Property(sn => sn.Title).HasMaxLength(200);
            entity.Property(sn => sn.Message).HasMaxLength(1000);
            entity.Property(sn => sn.CronExpression).HasMaxLength(100);

            entity.HasOne(sn => sn.TargetUser)
                .WithMany()
                .HasForeignKey(sn => sn.TargetUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // CurfewLog
        modelBuilder.Entity<CurfewLog>(entity =>
        {
            entity.HasIndex(cl => new { cl.StudentId, cl.CurfewTime });
            entity.HasQueryFilter(cl => !cl.IsDeleted);

            entity.HasOne(cl => cl.Student)
                .WithMany()
                .HasForeignKey(cl => cl.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cl => cl.Hostel)
                .WithMany()
                .HasForeignKey(cl => cl.HostelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Seed Data ----------
        var deptCS = new Guid("a1000000-0000-0000-0000-000000000001");
        var deptECE = new Guid("a1000000-0000-0000-0000-000000000002");
        var deptME = new Guid("a1000000-0000-0000-0000-000000000003");

        modelBuilder.Entity<Department>().HasData(
            new { Id = deptCS, Name = "Computer Science & Engineering", Code = "CSE", Description = "Department of Computer Science and Engineering", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = deptECE, Name = "Electronics & Communication", Code = "ECE", Description = "Department of Electronics and Communication Engineering", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = deptME, Name = "Mechanical Engineering", Code = "ME", Description = "Department of Mechanical Engineering", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );

        var adminId = new Guid("b1000000-0000-0000-0000-000000000001");
        var faculty1Id = new Guid("b1000000-0000-0000-0000-000000000002");
        var faculty2Id = new Guid("b1000000-0000-0000-0000-000000000003");
        var student1Id = new Guid("b1000000-0000-0000-0000-000000000004");
        var student2Id = new Guid("b1000000-0000-0000-0000-000000000005");
        var student3Id = new Guid("b1000000-0000-0000-0000-000000000006");
        var wardenId = new Guid("b1000000-0000-0000-0000-000000000007");
        var securityId = new Guid("b1000000-0000-0000-0000-000000000008");

        modelBuilder.Entity<User>().HasData(
            new { Id = adminId, Email = "admin@college.edu", FullName = "System Admin", Role = UserRole.Admin, IsActive = true, DepartmentId = (Guid?)deptCS, HostelId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = faculty1Id, Email = "faculty1@college.edu", FullName = "Dr. Priya Sharma", Role = UserRole.Faculty, EmployeeId = "FAC001", IsActive = true, DepartmentId = (Guid?)deptCS, HostelId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = faculty2Id, Email = "faculty2@college.edu", FullName = "Dr. Rajesh Kumar", Role = UserRole.Faculty, EmployeeId = "FAC002", IsActive = true, DepartmentId = (Guid?)deptECE, HostelId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = student1Id, Email = "student1@college.edu", FullName = "Rohith Reddy", Role = UserRole.Student, StudentId = "STU2024001", IsActive = true, DepartmentId = (Guid?)deptCS, HostelId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = student2Id, Email = "student2@college.edu", FullName = "Ananya Patel", Role = UserRole.Student, StudentId = "STU2024002", IsActive = true, DepartmentId = (Guid?)deptCS, HostelId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = student3Id, Email = "student3@college.edu", FullName = "Vikram Singh", Role = UserRole.Student, StudentId = "STU2024003", IsActive = true, DepartmentId = (Guid?)deptECE, HostelId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = wardenId, Email = "warden@college.edu", FullName = "Mr. Suresh Babu", Role = UserRole.Warden, EmployeeId = "WAR001", IsActive = true, DepartmentId = (Guid?)null, HostelId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = securityId, Email = "security@college.edu", FullName = "Mr. Ravi Kumar", Role = UserRole.Security, EmployeeId = "SEC001", IsActive = true, DepartmentId = (Guid?)null, HostelId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );

        var course1Id = new Guid("c1000000-0000-0000-0000-000000000001");
        var course2Id = new Guid("c1000000-0000-0000-0000-000000000002");
        var course3Id = new Guid("c1000000-0000-0000-0000-000000000003");

        modelBuilder.Entity<Course>().HasData(
            new { Id = course1Id, Name = "Data Structures & Algorithms", Code = "CS201", Description = "Core DSA course", Credits = 4, Semester = 3, Year = 2026, DepartmentId = deptCS, FacultyId = faculty1Id, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = course2Id, Name = "Database Management Systems", Code = "CS301", Description = "DBMS fundamentals", Credits = 3, Semester = 5, Year = 2026, DepartmentId = deptCS, FacultyId = faculty1Id, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = course3Id, Name = "Digital Signal Processing", Code = "ECE301", Description = "DSP fundamentals", Credits = 4, Semester = 5, Year = 2026, DepartmentId = deptECE, FacultyId = faculty2Id, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );

        modelBuilder.Entity<CourseEnrollment>().HasData(
            new { Id = new Guid("d1000000-0000-0000-0000-000000000001"), StudentId = student1Id, CourseId = course1Id, EnrolledAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("d1000000-0000-0000-0000-000000000002"), StudentId = student2Id, CourseId = course1Id, EnrolledAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("d1000000-0000-0000-0000-000000000003"), StudentId = student1Id, CourseId = course2Id, EnrolledAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("d1000000-0000-0000-0000-000000000004"), StudentId = student3Id, CourseId = course3Id, EnrolledAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );

        var hostel1Id = new Guid("e1000000-0000-0000-0000-000000000001");
        var hostel2Id = new Guid("e1000000-0000-0000-0000-000000000002");

        modelBuilder.Entity<Hostel>().HasData(
            new { Id = hostel1Id, Name = "Boys Hostel A", Block = "A", Capacity = 200, WardenId = (Guid?)wardenId, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = hostel2Id, Name = "Girls Hostel B", Block = "B", Capacity = 150, WardenId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );

        // Seed Badges
        modelBuilder.Entity<Badge>().HasData(
            new { Id = new Guid("f1000000-0000-0000-0000-000000000001"), Name = "First Scan", Description = "Marked attendance for the first time", Type = BadgeType.FirstScan, IconUrl = "🎉", SortOrder = 1, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f1000000-0000-0000-0000-000000000002"), Name = "Week Warrior", Description = "100% attendance for a full week", Type = BadgeType.WeekWarrior, IconUrl = "⚔️", RuleExpression = "streak >= 5", SortOrder = 2, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f1000000-0000-0000-0000-000000000003"), Name = "Monthly Champion", Description = "100% attendance for a full month", Type = BadgeType.MonthlyChampion, IconUrl = "🏆", RuleExpression = "streak >= 22", SortOrder = 3, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f1000000-0000-0000-0000-000000000004"), Name = "Streak Master", Description = "Maintained a 30-day attendance streak", Type = BadgeType.StreakMaster, IconUrl = "🔥", RuleExpression = "streak >= 30", SortOrder = 4, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f1000000-0000-0000-0000-000000000005"), Name = "On-Time Hero", Description = "Always on time, never late", Type = BadgeType.OnTimeHero, IconUrl = "⏰", RuleExpression = "late_count == 0", SortOrder = 5, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f1000000-0000-0000-0000-000000000006"), Name = "Perfect Attendance", Description = "100% attendance for the entire semester", Type = BadgeType.HundredPercent, IconUrl = "💯", RuleExpression = "attendance == 100", SortOrder = 6, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );

        // Seed System Configs
        modelBuilder.Entity<SystemConfig>().HasData(
            new { Id = new Guid("f2000000-0000-0000-0000-000000000001"), Key = "attendance.defaulter_threshold", Value = "75", Description = "Minimum attendance percentage before flagging as defaulter", DataType = "int", Category = ConfigCategory.Attendance, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f2000000-0000-0000-0000-000000000002"), Key = "qr.expiration_seconds", Value = "30", Description = "QR code expiration time in seconds", DataType = "int", Category = ConfigCategory.QR, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f2000000-0000-0000-0000-000000000003"), Key = "geofence.default_radius_meters", Value = "100", Description = "Default geofence radius in meters", DataType = "int", Category = ConfigCategory.Geofencing, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f2000000-0000-0000-0000-000000000004"), Key = "hostel.curfew_time", Value = "22:00", Description = "Hostel curfew time (24h format)", DataType = "string", Category = ConfigCategory.Hostel, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f2000000-0000-0000-0000-000000000005"), Key = "hostel.curfew_grace_minutes", Value = "15", Description = "Grace period after curfew before marking violation", DataType = "int", Category = ConfigCategory.Hostel, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f2000000-0000-0000-0000-000000000006"), Key = "security.max_devices_per_user", Value = "2", Description = "Maximum devices a user can bind", DataType = "int", Category = ConfigCategory.Security, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f2000000-0000-0000-0000-000000000007"), Key = "security.fraud_auto_flag", Value = "true", Description = "Automatically flag suspicious attendance", DataType = "bool", Category = ConfigCategory.Security, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("f2000000-0000-0000-0000-000000000008"), Key = "gamification.streak_bonus_multiplier", Value = "1.5", Description = "Score multiplier for active streaks", DataType = "string", Category = ConfigCategory.Gamification, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );
    }
}
