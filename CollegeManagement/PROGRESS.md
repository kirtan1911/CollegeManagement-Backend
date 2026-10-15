# College Management System — Progress Log

## Session 2 Audit (2026-07-08)

### Phase 1 — Subject Hierarchy ✅ DONE
- [x] `Course.cs` has `Semester` field (int, 1–8)
- [x] `CourseDTO.cs` includes `Semester`
- [x] `AdminController` GetCourses/AddCourse/UpdateCourse all handle `Semester` + unique check on (DeptId, Semester, CourseCode)
- [x] `StaffController.GetCourses` includes `Semester`, supports `?departmentId=` and `?semester=` filters
- [x] `StudentController.GetCourses` includes `Semester` and `Credits`
- [x] Migration `20260708131052_AddSemesterToCourse` generated and applied

### Phase 2 — Study Material Category ✅ DONE
- [x] `StudyMaterial.cs` has `MaterialCategory` enum, `Category`, `PracticalNumber?`, `ExternalLink?`
- [x] `UploadMaterialDTO.cs` has all new fields, `File` is nullable (optional for GoogleClassroomLink)
- [x] `StudyMaterialController.Upload` correctly branches: GoogleClassroomLink skips file, requires ExternalLink; Practical requires PracticalNumber > 0
- [x] `GetByCourse` supports `?category=` filter; Practicals sorted by PracticalNumber ASC, rest by UploadedAt DESC
- [x] Migration `20260708131428_AddStudyMaterialCategory` generated and applied

### Phase 3 — Marks Restructuring ✅ DONE
- [x] `InternalMark.cs` — LectureNoteMarks/VivaMarks REMOVED, `FacultyDiscretionMarks` (0–10) added
- [x] `UpdateInternalDTO.cs` — has `FacultyDiscretionMarks`, removed old fields
- [x] `InternalMarkController.cs` — validation 0–5/0–5/0–10, TotalInternal formula correct, capped at 40
- [x] `ExamController.cs` — CreateExam enforces TotalMarks=20 for Internal1/2, =60 for External
- [x] FIXED: `ReportController.cs` replaced deleted `LectureNoteMarks`/`VivaMarks` with `FacultyDiscretionMarks` + added `Semester`
- [x] Migration `20260708132857_RestructureInternalMarks` generated and applied

### Phase 4 — Exam Portal: Join Window, Fee Gating ✅ DONE (verified in ExamController.cs)
- [x] `ExamController.JoinExam` — 5-minute window (StartDateTime ±5min), correct Gujarati messages
- [x] Fee gating: checks any `IsPaid == false` FeeRecord before allowing join
- [x] One-attempt enforcement via ExamAttempts lookup — correct

### Phase 5 — Results, Grading, SPI & CGPA ✅ DONE
- [x] `GradeScale.cs` — GradeScaleId, MinMarks, MaxMarks, Grade, GradePoint (verified correct)
- [x] `GradeScale` seed data: A+(≥90/10), A(80-89/9), B+(70-79/8), B(60-69/7), C+(50-59/6), C(40-49/5), Fail(<40/0) — in `AppDbContext.HasData`
- [x] `ResultController.cs` — GET /api/Result/my (Student), GET /api/Result/student/{id} (Admin/Staff)
- [x] Grade computed from DB GradeScale table; SPI per semester (credit-weighted); CGPA overall
- [x] Migration `AddPhases5to11` generated and applied — GradeScales table seeded
- [ ] Frontend: results.html — NOT STARTED

### Phase 6 — Hostel Module ✅ DONE (backend)
- [x] `Hostel.cs` — Hostel, HostelRoom, HostelAllotment combined; all entities verified
- [x] FIXED (Session 4): added explicit `HasKey` for Hostel, HostelRoom, HostelAllotment — was root cause of migration failure
- [x] `HostelController.cs` — GET hostels, POST hostel, GET/POST rooms, POST allot, DELETE vacate, GET my-allotment, GET occupancy
- [x] Auto FeeRecord created on allotment, occupancy tracking, one-active-allotment-per-student check
- [x] `HostelDTO.cs` — HostelDTO, HostelRoomDTO, HostelAllotmentDTO
- [x] Migration `AddPhases5to11` includes Hostels/HostelRooms/HostelAllotments tables
- [ ] Frontend: hostel.html — NOT STARTED

### Phase 7 — Commute Details ✅ DONE (backend)
- [x] `StudentProfile.cs` — ResidenceType enum, HomeTown, TransportOpted, TransportRoute (nullable)
- [x] `StudentProfileDTO.cs` — updated with commute fields
- [x] Migration `AddPhases5to11` includes new StudentProfile columns
- [ ] StudentController.GetProfile does NOT yet expose commute fields — needs update
- [ ] Frontend: student.html / admin.html commute fields — NOT STARTED

### Phase 8 — Re-check / Re-evaluation ✅ DONE (backend)
- [x] `RecheckRequest.cs` — all fields verified (RequestId, StudentProfileId, ExamId, Reason, Status, OldMarks, NewMarks, ProcessedByUserId, ProcessedDate)
- [x] `RecheckController.cs` — POST request (7-day window), GET pending, PUT process (approve/reject, updates underlying marks, sends Notification), GET my-requests
- [x] `RecheckDTO.cs` — RecheckRequestDTO, ProcessRecheckDTO
- [x] Migration `AddPhases5to11` includes RecheckRequests table
- [ ] Frontend: recheck.html — NOT STARTED

### Phase 9 — Placement Module ✅ DONE (backend)
- [x] `Placement.cs` — Company, PlacementApplication, PlacementRecord (combined file, all three classes)
- [x] FIXED (Session 4): explicit HasKey for Company, PlacementApplication, PlacementRecord
- [x] `PlacementController.cs` — GET/POST/PUT/DELETE companies, POST apply, GET my-applications, PUT status (shortlist/select/reject + auto PlacementRecord + Notification), GET applications/{companyId}, GET placed-students
- [x] `PlacementDTO.cs` — CompanyDTO, PlacementStatusDTO
- [x] Migration `AddPhases5to11` includes Companies/PlacementApplications/PlacementRecords tables
- [ ] Frontend: placement.html — NOT STARTED

### Phase 10 — Time Table ✅ DONE (backend)
- [x] `TimeTableEntry.cs` — all fields verified (EntryId, DepartmentId, Semester, CourseId, DayOfWeek, StartTime, EndTime, RoomNumber, FacultyUserId)
- [x] FIXED (Session 4): explicit HasKey for TimeTableEntry
- [x] `TimeTableController.cs` — POST (with dept+sem+day+time overlap AND faculty+day+time conflict checks), DELETE, GET by dept/semester, GET my (student auto-lookup), GET my-schedule (faculty)
- [x] `TimeTableEntryDTO.cs` — verified correct
- [x] Migration `AddPhases5to11` includes TimeTableEntries table
- [ ] Frontend: timetable.html — NOT STARTED

### Phase 11 — Security Hardening ✅ DONE (backend)
- [x] `UsersController.cs` — [Authorize(Roles = "Admin")] added at class level
- [x] `UpdateUser` uses `UpdateUserDTO` (RoleName-based lookup) not raw `User` model
- [x] GET endpoints no longer leak PasswordHash (project to anon type)
- [x] `appsettings.json` — Jwt:Key and EmailSettings:AppPassword set to empty string (not committed)
- [x] `dotnet user-secrets set "Jwt:Key"` and `"EmailSettings:AppPassword"` — confirmed set
- [ ] `.gitignore` — MISSING from repo root; needs to be created
- [ ] `appsettings.Development.json` — doesn't exist yet; gitignore coverage needed

## Duplicates Found & Removed (Session 4)
- No duplicate class definitions found across files.
- No duplicate endpoint definitions found in controllers.
- AppDbContext had duplicate HostelAllotment FK config (HasKey was added in Sess3 but existing FK blocks kept). Session 4 replaced the entire hostel section with clean HasKey + FK configs — no duplicates remain.
- GradeScale seed data exists only in AppDbContext.HasData — confirmed single definition.

## Open Questions
- None — using all defaults from plan.

## Remaining Work (Session 4)
- [x] Migration applied — UNBLOCKED
- [ ] StudentController.GetProfile — add commute fields (Phase 7)
- [ ] AdminController — add commute fields to student update (Phase 7)
- [ ] .gitignore — create at repo root
- [ ] Frontend: results.html
- [ ] Frontend: hostel.html
- [ ] Frontend: recheck.html
- [ ] Frontend: placement.html
- [ ] Frontend: timetable.html
- [ ] Frontend: courses.html Semester column/filter (Phase 1)
- [ ] Frontend: study-materials.html category filter (Phase 2)
- [ ] Frontend: student.html / admin.html commute fields (Phase 7)
- [ ] Frontend: Exam-Portal.html join-window message display (Phase 4)