using CollegeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
namespace CollegeManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Existing DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Mark> Marks { get; set; }

        // New DbSets — Exam System / Fees / Materials / Notifications
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<ExamAttempt> ExamAttempts { get; set; }
        public DbSet<ExamAnswer> ExamAnswers { get; set; }
        public DbSet<InternalMark> InternalMarks { get; set; }
        public DbSet<ExternalMark> ExternalMarks { get; set; }
        public DbSet<StudyMaterial> StudyMaterials { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<FeeRecord> FeeRecords { get; set; }

        // Forgot Password OTP
        public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }

        // ── Phase 5: Grade Scale ──
        public DbSet<GradeScale> GradeScales { get; set; }

        // ── Phase 6: Hostel ──
        public DbSet<Hostel> Hostels { get; set; }
        public DbSet<HostelRoom> HostelRooms { get; set; }
        public DbSet<HostelAllotment> HostelAllotments { get; set; }

        // ── Phase 8: Re-check ──
        public DbSet<RecheckRequest> RecheckRequests { get; set; }

        // ── Phase 9: Placement ──
        public DbSet<Company> Companies { get; set; }
        public DbSet<PlacementApplication> PlacementApplications { get; set; }
        public DbSet<PlacementRecord> PlacementRecords { get; set; }

        // ── Phase 10: Time Table ──
        public DbSet<TimeTableEntry> TimeTableEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Unique index on Users.Email (prevents race-condition duplicate registrations) ──
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ── Unique index on StudentProfiles.EnrollmentNo ──
            modelBuilder.Entity<StudentProfile>()
                .HasIndex(sp => sp.EnrollmentNo)
                .IsUnique();

            // ── Course unique constraint: (DepartmentId, Semester, CourseCode) ──
            modelBuilder.Entity<Course>()
                .HasIndex(c => new { c.DepartmentId, c.Semester, c.CourseCode })
                .IsUnique();

            // ── Existing relationships ──

            // Enrollment -> StudentProfile : No Cascade
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.StudentProfile)
                .WithMany(sp => sp.Enrollments)
                .HasForeignKey(e => e.StudentProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // Enrollment -> Course : No Cascade
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Attendance -> Enrollment : No Cascade
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Enrollment)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EnrollmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Mark -> Enrollment : No Cascade
            modelBuilder.Entity<Mark>()
                .HasOne(m => m.Enrollment)
                .WithMany(e => e.Marks)
                .HasForeignKey(m => m.EnrollmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // StudentProfile -> Department : No Cascade
            modelBuilder.Entity<StudentProfile>()
                .HasOne(sp => sp.Department)
                .WithMany()
                .HasForeignKey(sp => sp.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── New relationships — Exam System ──

            // Exam -> Course : No Cascade
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Question -> Exam : No Cascade
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.NoAction);

            // ExamAttempt -> Exam : No Cascade
            modelBuilder.Entity<ExamAttempt>()
                .HasOne(a => a.Exam)
                .WithMany(e => e.Attempts)
                .HasForeignKey(a => a.ExamId)
                .OnDelete(DeleteBehavior.NoAction);

            // ExamAttempt -> StudentProfile : No Cascade
            modelBuilder.Entity<ExamAttempt>()
                .HasOne(a => a.StudentProfile)
                .WithMany()
                .HasForeignKey(a => a.StudentProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // ExamAnswer -> ExamAttempt : No Cascade
            modelBuilder.Entity<ExamAnswer>()
                .HasOne(a => a.Attempt)
                .WithMany(at => at.Answers)
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.NoAction);

            // ExamAnswer -> Question : No Cascade
            modelBuilder.Entity<ExamAnswer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── New relationships — Internal / External Marks ──

            // InternalMark -> StudentProfile : No Cascade
            modelBuilder.Entity<InternalMark>()
                .HasOne(im => im.StudentProfile)
                .WithMany()
                .HasForeignKey(im => im.StudentProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // InternalMark -> Course : No Cascade
            modelBuilder.Entity<InternalMark>()
                .HasOne(im => im.Course)
                .WithMany()
                .HasForeignKey(im => im.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ExternalMark -> StudentProfile : No Cascade
            modelBuilder.Entity<ExternalMark>()
                .HasOne(em => em.StudentProfile)
                .WithMany()
                .HasForeignKey(em => em.StudentProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // ExternalMark -> Course : No Cascade
            modelBuilder.Entity<ExternalMark>()
                .HasOne(em => em.Course)
                .WithMany()
                .HasForeignKey(em => em.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── New relationships — StudyMaterial ──

            // StudyMaterial -> Course : No Cascade
            modelBuilder.Entity<StudyMaterial>()
                .HasOne(sm => sm.Course)
                .WithMany()
                .HasForeignKey(sm => sm.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Notification & FeeRecord ──

            // FeeRecord -> StudentProfile : No Cascade
            modelBuilder.Entity<FeeRecord>()
                .HasOne(f => f.StudentProfile)
                .WithMany()
                .HasForeignKey(f => f.StudentProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // Notification has no FK navigation (UserId is plain int, no nav property) — no config needed

            // ── Forgot Password OTP ──

            // PasswordResetOtp -> User : Cascade
            modelBuilder.Entity<PasswordResetOtp>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // InternalMark
            modelBuilder.Entity<InternalMark>()
                .Property(im => im.Internal1Score).HasPrecision(10, 2);
            modelBuilder.Entity<InternalMark>()
                .Property(im => im.Internal2Score).HasPrecision(10, 2);
            modelBuilder.Entity<InternalMark>()
                .Property(im => im.BestInternalScore).HasPrecision(10, 2);
            modelBuilder.Entity<InternalMark>()
                .Property(im => im.AttendanceMarks).HasPrecision(10, 2);
            modelBuilder.Entity<InternalMark>()
                .Property(im => im.QuizMarks).HasPrecision(10, 2);
            modelBuilder.Entity<InternalMark>()
                .Property(im => im.FacultyDiscretionMarks).HasPrecision(10, 2);
            modelBuilder.Entity<InternalMark>()
                .Property(im => im.TotalInternal).HasPrecision(10, 2);

            // ExternalMark
            modelBuilder.Entity<ExternalMark>()
                .Property(em => em.ExternalScore).HasPrecision(10, 2);

            // Mark
            modelBuilder.Entity<Mark>()
                .Property(m => m.ObtainedMarks).HasPrecision(10, 2);
            modelBuilder.Entity<Mark>()
                .Property(m => m.TotalMarks).HasPrecision(10, 2);

            // Question
            modelBuilder.Entity<Question>()
                .Property(q => q.Marks).HasPrecision(10, 2);

            // ExamAttempt
            modelBuilder.Entity<ExamAttempt>()
                .Property(a => a.TotalObtained).HasPrecision(10, 2);

            // RecheckRequest
            modelBuilder.Entity<RecheckRequest>()
                .Property(r => r.OldMarks).HasPrecision(10, 2);
            modelBuilder.Entity<RecheckRequest>()
                .Property(r => r.NewMarks).HasPrecision(10, 2);

            // PlacementRecord
            modelBuilder.Entity<PlacementRecord>()
                .Property(p => p.PackageOffered).HasPrecision(18, 2);

            // Company
            modelBuilder.Entity<Company>()
                .Property(c => c.PackageLPA).HasPrecision(18, 2);

            // GradeScale
            modelBuilder.Entity<GradeScale>()
                .Property(g => g.MinMarks).HasPrecision(10, 2);
            modelBuilder.Entity<GradeScale>()
                .Property(g => g.MaxMarks).HasPrecision(10, 2);
            modelBuilder.Entity<GradeScale>()
                .Property(g => g.GradePoint).HasPrecision(10, 2);

            // ── Phase 5: GradeScale — seed default data ──
            modelBuilder.Entity<GradeScale>().HasData(
                new GradeScale { GradeScaleId = 1, MinMarks = 90, MaxMarks = 100, Grade = "A+", GradePoint = 10 },
                new GradeScale { GradeScaleId = 2, MinMarks = 80, MaxMarks = 89,  Grade = "A",  GradePoint = 9 },
                new GradeScale { GradeScaleId = 3, MinMarks = 70, MaxMarks = 79,  Grade = "B+", GradePoint = 8 },
                new GradeScale { GradeScaleId = 4, MinMarks = 60, MaxMarks = 69,  Grade = "B",  GradePoint = 7 },
                new GradeScale { GradeScaleId = 5, MinMarks = 50, MaxMarks = 59,  Grade = "C+", GradePoint = 6 },
                new GradeScale { GradeScaleId = 6, MinMarks = 40, MaxMarks = 49,  Grade = "C",  GradePoint = 5 },
                new GradeScale { GradeScaleId = 7, MinMarks = 0,  MaxMarks = 39,  Grade = "Fail", GradePoint = 0 }
            );

            // ── Phase 6: Hostel — explicit keys for all entities (multi-class file) ──
            modelBuilder.Entity<Hostel>().HasKey(h => h.HostelId);
            modelBuilder.Entity<HostelRoom>().HasKey(r => r.RoomId);
            modelBuilder.Entity<HostelAllotment>().HasKey(a => a.AllotmentId);

            modelBuilder.Entity<HostelRoom>()
                .HasOne(r => r.Hostel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HostelId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<HostelAllotment>()
                .HasOne(a => a.Room)
                .WithMany(r => r.Allotments)
                .HasForeignKey(a => a.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<HostelAllotment>()
                .HasOne(a => a.StudentProfile)
                .WithMany()
                .HasForeignKey(a => a.StudentProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Phase 8: RecheckRequest — explicit key ──
            modelBuilder.Entity<RecheckRequest>().HasKey(r => r.RequestId);

            modelBuilder.Entity<RecheckRequest>()
                .HasOne(r => r.StudentProfile)
                .WithMany()
                .HasForeignKey(r => r.StudentProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RecheckRequest>()
                .HasOne(r => r.Exam)
                .WithMany()
                .HasForeignKey(r => r.ExamId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Phase 9: Placement — explicit keys for all entities (multi-class file) ──
            modelBuilder.Entity<Company>().HasKey(c => c.CompanyId);
            modelBuilder.Entity<PlacementApplication>().HasKey(a => a.ApplicationId);
            modelBuilder.Entity<PlacementRecord>().HasKey(r => r.PlacementId);

            modelBuilder.Entity<PlacementApplication>()
                .HasOne(a => a.StudentProfile)
                .WithMany()
                .HasForeignKey(a => a.StudentProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PlacementApplication>()
                .HasOne(a => a.Company)
                .WithMany(c => c.Applications)
                .HasForeignKey(a => a.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PlacementRecord>()
                .HasOne(r => r.StudentProfile)
                .WithMany()
                .HasForeignKey(r => r.StudentProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PlacementRecord>()
                .HasOne(r => r.Company)
                .WithMany()
                .HasForeignKey(r => r.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Phase 10: TimeTableEntry — explicit key ──
            modelBuilder.Entity<TimeTableEntry>().HasKey(t => t.EntryId);

            modelBuilder.Entity<TimeTableEntry>()
                .HasOne(t => t.Department)
                .WithMany()
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TimeTableEntry>()
                .HasOne(t => t.Course)
                .WithMany()
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
