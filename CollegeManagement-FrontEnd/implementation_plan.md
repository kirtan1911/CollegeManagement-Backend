# College Management System — Glass Dashboard Reskin + New Pages

## Overview
Full frontend overhaul of the existing College Management System from an indigo/dark flat theme to a **"Glass Dashboard"** aesthetic: cinematic bokeh dark background, frosted-glass cards, warm orange→coral accent, floating glass sidebar, and 9 pages total (4 reskin + 5 new builds).

## Proposed Changes

---

### Shared Foundation

#### [NEW] `glass-theme.css`
One shared stylesheet imported by every page. Contains:
- CSS custom properties (all color tokens, blur values, radii)
- Cinematic dark background with CSS gradient + pseudo-noise bokeh layer (no stock photo)
- `.glass-card`, `.glass-sidebar`, `.glass-stat-card`, `.glass-btn-primary`, `.glass-btn-secondary`
- `.glass-input`, `.glass-select`, `.glass-table`, `.glass-modal`, `.glass-badge`, `.glass-chip-filter`
- Empty-state component, toast notification
- Responsive rules: sidebar collapses to bottom glass dock below 768px
- Orange glow focus ring (replaces Bootstrap blue)

#### [NEW] `glass-theme.js`
Shared utilities:
- `toast(msg, type)` — floating glass toast
- `showGlassModal(title, html, buttons)` — glass-reskinned modal helper
- `authHeader()` — returns Bearer header from localStorage
- `requireAuth(roles)` — redirect guard
- `badgeHtml(status)` — returns `.glass-badge` HTML for status strings

---

### Phase A — Reskin Existing Pages

#### [MODIFY] `courses.html`
- Replace indigo theme CSS with glass-theme import
- Add **Semester** column to table header + rows (data from API already returns `semester`)
- Add **Semester filter** as `.glass-chip-filter` row (All / 1–8 pills)
- Department filter converted to `.glass-select`
- All buttons → `.glass-btn-primary` / `.glass-btn-secondary`
- Modal → `.glass-modal`
- Sidebar → `.glass-sidebar` icon-rail variant (inline since courses.html has its own sidebar)
- `deleteCourse()` now uses `showGlassModal` confirm instead of `confirm()`

#### [MODIFY] `StudyMaterial.html`
- Replace flat CSS with glass-theme
- Filter panel: 3-level cascade `Department → Semester → Subject` using `.glass-select`
  - Dept loads on page init from `/api/Admin/departments` or `/api/Staff/courses`  
  - Semester chips auto-populate based on dept selection
  - Subject (course) select populates based on dept+semester  
- **Category filter** as `.glass-chip-filter` row: All / Practical / LectureNote / PPT / GoogleClassroomLink / Image / Other
- Upload modal: conditional inputs:
  - Category = `Practical` → show `PracticalNumber` input
  - Category = `GoogleClassroomLink` → show `ExternalLink` input, hide file picker
  - Others → show file picker only
- `deleteMaterial()` → glass confirm modal

#### [MODIFY] `Exam-Portal.html`
- Replace indigo accent with orange-coral gradient throughout
- Bokeh background layers
- `joinExam()` error handler: instead of `showAlert(boxId, msg, "error")` show **glass toast** using `toast(msg, 'error')` — or optionally a glass modal for critical errors (fee-gating, window-closed Gujarati messages)
- `confirmSubmit()` → glass modal instead of `confirm()`
- All `.alert-box` → styled as glass alert pills
- Result circle → orange gradient border instead of indigo
- Option cards → glass hover style
- Timer box → glass pill

#### [MODIFY] `student.html` — Commute fields section
- Add commute section to **My Profile** panel (below existing fields):
  - `ResidenceType` dropdown: Local / Commuter / Hosteler
  - `HomeTown` text input (always visible)
  - `TransportOpted` toggle checkbox with label
  - `TransportRoute` text input (only visible when TransportOpted = true)
- API: `GET /api/Student/profile` already returns these; `PUT` endpoint to be wired
- Reskin entire page to glass theme (sidebar, nav, cards, tables)

---

### Phase B — New Pages

#### [OVERWRITE] `results.html`
Already partially built in the old indigo theme — **full rewrite** to glass theme:
- CGPA stat card (orange gradient circle)
- SPI per-semester stat cards as `.glass-stat-card` pills in a horizontal scroll row
- Semester selector as `.glass-chip-filter` (clicking filters the table below)
- Subject table as `.glass-table` with Pass/Fail `.glass-badge`
- Grade pill colors: green (A/A+), amber (B/B+/C+), red (Fail)
- Admin/Staff: student picker dropdown with "Load Results" button
- Auto-loads for Student role
- Endpoints: `GET /api/Result/my`, `GET /api/Result/student/{id}`

#### [NEW] `hostel.html`
- **Admin view** (role=Admin/Staff):
  - Occupancy stat cards: total capacity, occupied, available per hostel
  - Hostel CRUD table (`.glass-table`) — Add hostel modal
  - Room management table per hostel — Add room modal
  - Allotment table — student picker + room picker + "Allot" button
  - Vacate button per active allotment
- **Student view** (role=Student):
  - "My Allotment" `.glass-stat-card` (room, hostel name, fee status)
  - "Request Allotment" — redirects student to contact admin (no direct API)
- Endpoints: `GET /api/Hostel`, `POST /api/Hostel`, `GET /api/Hostel/rooms`, `POST /api/Hostel/rooms`, `POST /api/Hostel/allot`, `DELETE /api/Hostel/vacate/{id}`, `GET /api/Hostel/my-allotment`, `GET /api/Hostel/occupancy`

#### [NEW] `recheck.html`
- **Student view**:
  - Submit form: select from student's submitted exams (`/api/Exam/my-attempts`), reason textarea
  - 7-day window enforced server-side; Gujarati error from API shown in `.glass-modal`
  - "My Requests" table with status `.glass-badge` (Pending/Approved/Rejected)
- **Admin/Staff/Faculty view**:
  - Pending requests `.glass-table`
  - Process modal: shows OldMarks, NewMarks input, Approve/Reject buttons
- Endpoints: `POST /api/Recheck/request`, `GET /api/Recheck/pending`, `PUT /api/Recheck/process/{id}`, `GET /api/Recheck/my-requests`

#### [NEW] `placement.html`
- **Admin/Staff view**:
  - Company CRUD grid (glass cards — name, role, package, drive date, eligibility)
  - "View Applicants" per company → applicants table with status update dropdown
  - "Placed Students" tab → placed students report table
- **Student view**:
  - Browse company cards grid (package, drive date, eligibility)
  - Apply button (disabled if already applied)
  - "My Applications" status list with `.glass-badge`
- Endpoints: all `/api/Placement/*` endpoints per controller

#### [NEW] `timetable.html`
- **Weekly grid** (Mon–Sat columns × time rows): glass cells, occupied cell = mini chip (Subject + Room + Faculty)
- **Admin/Staff** → Add/Edit/Delete entry modal, conflict errors in glass modal
- **Student** → read-only, auto-loads via `GET /api/TimeTable/my`
- **Faculty/Staff** → read-only via `GET /api/TimeTable/my-schedule`
- Admin → `POST /api/TimeTable`, `DELETE /api/TimeTable/{id}`, `GET /api/TimeTable/{deptId}/{semester}`

---

## Verification Plan

### Automated
- None (pure frontend HTML/JS)

### Manual
1. Open each page, confirm glass visual aesthetic loads (bokeh bg, frosted cards)
2. Test auth redirect for each page with wrong role
3. Test CRUD operations (add/edit/delete) wire to correct endpoints
4. Test role-specific views (Student vs Admin vs Faculty)
5. Verify Gujarati API error messages render in glass toasts/modals (not plain `alert()`)
6. Test responsive collapse below 768px (sidebar → bottom dock)

## Assumptions
- `glass-theme.css` + `glass-theme.js` in same directory as HTML files
- `admin.html` commute field reskin is deferred — user indicated `student.html` and `admin.html`; will add commute fields to both but student.html is primary
- Student timetable uses `/api/TimeTable/my`; faculty uses `/api/TimeTable/my-schedule`; admin uses picker
- `StudyMaterial.html` category filter hits `GET /api/StudyMaterial/course/{id}?category=X`
- Chart.js added only if visual trend charts are needed in results (SPI trend chart added as bonus if data exists)
