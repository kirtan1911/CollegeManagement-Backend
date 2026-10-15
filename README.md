# College Management System — Backend API

A production-quality **ASP.NET Core 8 Web API** for managing a college — students, faculty, exams, results, fees, hostel, placement, timetable, study materials, and more.

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| Runtime | ASP.NET Core 8.0 |
| ORM | Entity Framework Core 8 + SQL Server |
| Auth | JWT Bearer tokens |
| Password hashing | BCrypt.Net-Next |
| Email | MailKit (Gmail SMTP) |
| API Docs | Swagger (Swashbuckle) |
| Frontend | HTML + CSS + jQuery + Bootstrap 5 |

---

## ⚙️ Setup — Local Development

### 1. Clone
```bash
git clone https://github.com/kirtan1911/CollegeManagement-Backend.git
cd CollegeManagement-Backend
```

### 2. Configure secrets (never committed to git)

Create `CollegeManagement/appsettings.Development.json`:
```json
{
  "Jwt": {
    "Key": "YOUR_STRONG_SECRET_KEY_MIN_32_CHARS"
  },
  "EmailSettings": {
    "SenderEmail": "your-gmail@gmail.com",
    "AppPassword": "your-gmail-app-password"
  }
}
```

Or use **dotnet user-secrets**:
```bash
cd CollegeManagement
dotnet user-secrets set "Jwt:Key" "YOUR_STRONG_SECRET_KEY"
dotnet user-secrets set "EmailSettings:AppPassword" "your-gmail-app-password"
dotnet user-secrets set "EmailSettings:SenderEmail" "your-gmail@gmail.com"
```

### 3. Apply database migrations
```bash
cd CollegeManagement
dotnet ef database update
```

### 4. Run
```bash
dotnet run
# API available at https://localhost:7142
# Swagger UI at https://localhost:7142/swagger
```

---

## 📋 Modules

| Module | Endpoint Prefix | Roles |
|---|---|---|
| Authentication | `/api/Auth` | Public |
| Admin Dashboard | `/api/Admin` | Admin |
| Staff / Faculty | `/api/Staff` | Staff |
| Student Portal | `/api/Student` | Student |
| Exams | `/api/Exam` | All |
| Internal Marks | `/api/InternalMark` | Staff, Admin |
| Results & CGPA | `/api/Result` | Student, Admin, Staff |
| Study Material | `/api/StudyMaterial` | All |
| Notifications | `/api/Notification` | All |
| Fees | `/api/Fee` | Admin, Student |
| Hostel | `/api/Hostel` | Admin, Student |
| Re-check | `/api/Recheck` | Student, Admin |
| Placement | `/api/Placement` | All |
| Timetable | `/api/TimeTable` | All |
| Reports | `/api/Report` | Admin, Staff |
| Users | `/api/Users` | Admin |

---

## 🔐 Security

- JWT tokens with role-based authorization (`Admin`, `Staff`, `Student`)
- BCrypt password hashing
- Secrets managed via `appsettings.Development.json` (gitignored) or `dotnet user-secrets`
- Unique DB index on `Users.Email` and `StudentProfiles.EnrollmentNo`
- Input validation via Data Annotations + ModelState checks

---

## 📁 Project Structure

```
CollegeManagement/
├── Controllers/          # All API controllers
├── DTOs/                 # Data Transfer Objects
├── Models/               # EF Core entity models + AppDbContext
├── Migrations/           # EF Core database migrations
├── Services/             # JwtService, EmailService
├── appsettings.json      # Template (no secrets)
└── Program.cs            # App startup + DI registration
```

---

## 🛠️ Environment Requirements

- .NET 8 SDK
- SQL Server LocalDB (or SQL Server)
- Gmail account with App Password enabled (for email OTP)
