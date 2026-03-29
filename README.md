# College Attendance — Campus Management Platform

A production-grade, full-stack campus management system for colleges and universities. Features QR-based attendance tracking, hostel management, gamification, leave management, emergency SOS, fraud detection, and real-time analytics.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend API** | .NET 10, ASP.NET Core Web API, EF Core (SQL Server), SignalR |
| **Web Frontend** | React 19 + TypeScript, Vite, TailwindCSS v4, TanStack Query v5 |
| **Mobile App** | .NET MAUI (Android/iOS), CommunityToolkit.Mvvm, ZXing barcode scanner |
| **Auth** | JWT + Google OAuth 2.0, role-based authorization |
| **Architecture** | Clean Architecture (Domain → Application → Infrastructure → API) |

## Project Structure

```
college-attendance/
├── src/
│   ├── CollegeAttendance.Domain/          # Entities, Enums
│   │   └── Entities/                      # 25 entity classes
│   ├── CollegeAttendance.Application/     # DTOs, Interfaces, Services
│   │   ├── DTOs/                          # 80+ DTOs
│   │   ├── Interfaces/                    # 17 service interfaces
│   │   └── Services/                      # 13 service implementations
│   ├── CollegeAttendance.Infrastructure/  # EF DbContext, Repos, Email, Background jobs
│   │   ├── Data/                          # AppDbContext + seed data
│   │   ├── Repositories/                  # Generic Repository + UnitOfWork
│   │   └── Services/                      # EmailService, 3 HostedServices
│   ├── CollegeAttendance.API/             # Controllers, Hubs, Middleware
│   │   ├── Controllers/                   # 20 API controllers
│   │   ├── Hubs/                          # SignalR AttendanceHub
│   │   └── Middleware/                    # Exception handling
│   ├── college-attendance-web/            # React SPA
│   │   └── src/
│   │       ├── pages/                     # 25 pages (admin, faculty, student, hostel)
│   │       ├── services/                  # Axios API layer
│   │       ├── hooks/                     # TanStack Query hooks
│   │       └── components/               # Shared components (Sidebar, Layout)
│   └── CollegeAttendance.Mobile/          # .NET MAUI app
│       ├── Models/                        # Enums (17), DTOs (24+ classes)
│       ├── Services/                      # ApiService, AuthService, LocationService
│       ├── ViewModels/                    # 11 ViewModels (MVVM)
│       └── Views/                         # 11 XAML pages
└── CollegeAttendance.slnx                 # Solution file
```

## Modules & Features

### Core Attendance
- **QR-Based Attendance** — Faculty generate time-limited, geofenced QR codes; students scan to mark attendance
- **Class Sessions** — Create, manage, and track sessions with auto-expiry
- **Attendance Records** — Mark present/absent/late/excused with geolocation validation
- **Offline Sync** — Queue attendance locally, sync when connectivity resumes

### Academic Management
- **Departments** — CRUD with HOD assignment
- **Courses** — Course management with enrollment tracking
- **Faculty Sessions** — Per-faculty session history and attendance stats

### Hostel Intelligence
- **Hostel Management** — Buildings, rooms, wardens
- **Mess Tracking** — Meal logging (breakfast, lunch, snacks, dinner)
- **Hostel Logs** — Entry/exit tracking with check-in/check-out timestamps

### Student Engagement
- **Gamification Engine** — Badges, streaks, leaderboards, attendance scoring
- **Outing Requests** — Students request, wardens approve/reject with time tracking
- **Leave Management** — Medical/personal/academic/family leave with approval workflow
- **Emergency SOS** — One-tap panic button with GPS, priority levels, security response tracking

### Administration
- **User Management** — 7 roles (Admin, Faculty, Student, HOD, Warden, LabAssistant, Parent)
- **Bulk Import** — CSV upload for users, courses, enrollments
- **System Configuration** — Runtime-configurable settings by category (General, Attendance, Security, Hostel, etc.)
- **Curfew Management** — Hostel curfew rules with automated monitoring

### Security & Compliance
- **Fraud Detection** — Multi-location scan, device spoofing, rapid succession, IP anomaly detection
- **Device Binding** — Lock attendance to registered devices
- **Audit Logging** — Comprehensive action tracking
- **Geofencing** — Campus boundary enforcement for QR scans

### Analytics & Reporting
- **Dashboard Analytics** — Real-time stats for all roles
- **Advanced Analytics** — Trend analysis, department comparisons, predictive insights
- **Attendance Reports** — Per-student, per-course, per-department breakdowns

### Infrastructure
- **Real-Time Updates** — SignalR hub for live attendance notifications
- **Email Notifications** — SMTP-based alerts for approvals, emergencies, reminders
- **Background Services** — Attendance reminders, curfew monitoring, streak calculation
- **Swagger/OpenAPI** — Full API documentation at `/swagger`

## API Endpoints (20 Controllers)

| Controller | Base Route | Purpose |
|-----------|-----------|---------|
| Auth | `/api/auth` | Login, Google OAuth, refresh tokens |
| Users | `/api/users` | User CRUD, role management |
| Departments | `/api/departments` | Department CRUD |
| Courses | `/api/courses` | Course CRUD, enrollments |
| ClassSessions | `/api/sessions` | Session lifecycle |
| QR | `/api/qr` | Generate/validate QR codes |
| Attendance | `/api/attendance` | Mark, query, reports |
| Outing | `/api/outings` | Request/approve outings |
| Hostel | `/api/hostel` | Hostel & room management |
| Mess | `/api/mess` | Meal logging |
| Notifications | `/api/notifications` | Push notifications |
| Gamification | `/api/gamification` | Badges, streaks, leaderboard |
| Leave | `/api/leave` | Leave requests & approvals |
| Emergency | `/api/emergency` | SOS alerts & response |
| Fraud | `/api/fraud` | Fraud logs & detection |
| OfflineSync | `/api/sync` | Offline data synchronization |
| AdminConfig | `/api/admin/config` | System configuration |
| BulkImport | `/api/admin/import` | CSV bulk imports |
| Analytics | `/api/analytics` | Dashboard analytics |
| AdvancedAnalytics | `/api/analytics/advanced` | Trend & predictive analytics |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (for React frontend)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (LocalDB or full instance)
- [.NET MAUI workload](https://learn.microsoft.com/en-us/dotnet/maui/) (for mobile app)

### Backend Setup

```bash
cd src/CollegeAttendance.API

# Update connection string in appsettings.json
# Configure JWT settings (Jwt:Key, Jwt:Issuer, Jwt:Audience)
# Configure Google OAuth (Google:ClientId, Google:ClientSecret) — optional

# Apply EF migrations
dotnet ef database update --project ../CollegeAttendance.Infrastructure

# Run the API
dotnet run
```

The API starts at `http://localhost:5172` (HTTP) / `https://localhost:7168` (HTTPS).
Swagger UI available at `https://localhost:7168/swagger`.

### Frontend Setup

```bash
cd src/college-attendance-web

npm install
npm run dev
```

The web app starts at `http://localhost:5173`.

### Mobile App Setup

```bash
cd src/CollegeAttendance.Mobile

# Update API base URL in AppConstants.cs
dotnet build -f net10.0-android   # Android
dotnet build -f net10.0-ios       # iOS (macOS only)
```

### Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CollegeAttendance;Trusted_Connection=true"
  },
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-minimum-32-chars",
    "Issuer": "CollegeAttendance",
    "Audience": "CollegeAttendance"
  },
  "Google": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@college.edu",
    "FromName": "Campus Attendance System"
  }
}
```

## Entity Model (25 Entities)

User, Department, Course, CourseEnrollment, ClassSession, QRSession, AttendanceRecord, OutingRequest, Notification, ScheduledNotification, Hostel, HostelLog, MessLog, Badge, StudentBadge, AttendanceStreak, LeaderboardEntry, LeaveRequest, EmergencySOS, FraudLog, DeviceBinding, OfflineSyncLog, SystemConfig, CurfewLog, AuditLog

## Mobile App (MAUI)

8-tab navigation: Home, Scan, Attendance, Outings, Rewards, Leave, SOS, Profile

| Page | Features |
|------|----------|
| Dashboard | Role-based stats, quick actions |
| QR Scan | ZXing camera scanner, geolocation validation |
| Attendance | Session list with attendance status |
| Outings | Request/view outings with approval status |
| Gamification | Streaks, badges, leaderboard with period filters |
| Leave | Submit leave requests, track approval status |
| Emergency SOS | One-tap SOS with GPS, priority levels, alert history |
| Profile | User info, auth management |

## Build Status

- **Backend**: 0 errors, 0 warnings
- **React Frontend**: 0 errors, 0 warnings
- **MAUI Mobile**: Code complete

## License

MIT