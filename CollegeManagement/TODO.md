College Management System - Project Status & Debug Notes
Project Update - Exam Management & Report Module
✅ Exam Module - Completed
Files Modified
ExamController.cs
Exam.html
Exam-Portal.html
Faculty.html
admin.html
users.html
departments.html
courses.html
students.html
student.html
Bugs Fixed
Bug 1 - Student Secret Code Visibility

Problem

Pehla GET /api/Exam/list/{courseId} badha users ne SecretCode return karto hato.

Aena karane koi pan student browser Network tab mathi secret code joi sakto hato.

Solution

ExamController ma role check add karyo.

Only

Admin
Faculty
Staff

aa loko ne SecretCode return thay.

Students mate SecretCode remove kari didho.

Bug 2 - Exam Attempts API Missing

Faculty ne exam attempts jovani facility hati nahi.

Solution

Navo endpoint add karyo.

GET /api/Exam/{examId}/attempts

Have Faculty/Admin badha student attempts joi shake che.

Bug 3 - Bootstrap Modal Warning

Browser warning aavti hati

Blocked aria-hidden on an element because its descendant retained focus

Reason:

Dar vakhat

new bootstrap.Modal(...)

create thatu hatu.

Close button focus ma rehto.

Solution

Modal reuse kari.

bootstrap.Modal.getOrCreateInstance(...)

ane

hide.bs.modal

event ma

document.activeElement.blur();

call karyu.

Warning completely solve thai gai.

Course Module Bug
Problem

Course model ma

public int Credits { get; set; }

property add kari hati.

Pan SQL Server table ma

Credits

column exist karto n hato.

EF Core automatically badhi public properties map kare che.

Aena karane

GET /courses
POST /courses

banne ma

500 Internal Server Error

aavto hato.

Solution
Step 1

AppDbContext.cs

modelBuilder.Entity<Course>()
            .Ignore(c => c.Credits);

Add karyu.

EF have Credits database sathe map karto nathi.

Step 2

AdminController

Projection ma

Credits = 0

set karyu.

Etle SQL query Credits column read karvano try karti nathi.

Step 3

Department navigation mate null safety pan add kari.

Bootstrap Modal Fix

Pehla

new bootstrap.Modal(...)

Dar click par navo modal banavto hato.

Have

bootstrap.Modal.getOrCreateInstance(...)

use kariye che.

Etle duplicate modal instances create thata nathi.

Backdrop issue pan solve thai gayu.

400 Bad Request Debug Steps

Jo browser ma

400 Bad Request

aave to aa order follow karvo.

Step 1

F12 kholo.

Network tab open karo.

Step 2

Red request select karo.

Headers ma

Request URL

check karo.

Jo

undefined

NaN

empty value

dekhay to frontend wrong data mokle che.

Step 3

Response tab kholo.

Backend exact validation error aapse.

Step 4

Backend Controller kholo.

Je endpoint call thay che e method shodho.

Parameters check karo.

Example

int id

DTO

FromBody
Step 5

Frontend payload compare karo backend sathe.

Check karo

field name
spelling
datatype
null value

Example

courseId

CourseId

banne same hova joie.

Common Reasons
Required field missing
Wrong datatype
undefined value
null value
DTO mismatch
JSON invalid

Shortcut

Network Tab

↓

Request Payload

↓

Backend DTO

↓

Mismatch = 99% 400 Error
Report Module Debug Notes
Error 1
GET /api/StudentProfile

404

Reason

StudentProfileController project ma j nathi.

Frontend wrong endpoint call karto hato.

Wrong

/StudentProfile

Correct

/Admin/student-profiles
Error 2
GET /api/admin/students

404

Reason

Actual route

/Admin/student-profiles

che.

/students

route exist karto nathi.

Error 3

Dropdown ma Student Name empty.

Reason

Backend response

studentName

return kare che.

Frontend

name

search karto hato.

Fix

studentName

↓

StudentName

↓

name

↓

Name

aa order ma check karvanu.

Error 4

Student login pachi

GET /StudentProfile/my

404

Reason

StudentController ma

/my

route nathi.

Correct route

/Student/profile
Pending Issues
Issue 5

Student login pachi

No students found

aave che.

Possible reason

StudentProfiles

table empty hoy.

SQL run karo.

SELECT *
FROM StudentProfiles

Jo record nathi to insert karo.

Issue 6

Report empty aave che.

Possible reason

Aa tables ma data nathi.

Check karo

InternalMarks
ExternalMarks
Attendance
FeeRecords

Jo empty hoy to dummy data insert karo.

Files Updated
report.html
STUDENT_LIST_ENDPOINTS fix
parseStudentList fix
Student profile endpoint fix
ReportController.cs

No changes.

AdminController.cs

No changes.

StudentController.cs

No changes.

API Routes Reference
Admin
GET /api/Admin/student-profiles
GET /api/Admin/dashboard
Student
GET /api/Student/profile
GET /api/Student/courses
GET /api/Student/attendance
GET /api/Student/marks
Report
GET /api/Report/student/{id}

(Admin / Faculty / Student)

GET /api/Report/course/{id}

(Admin / Faculty)

GET /api/Report/fee-summary

(Admin)

Next Priority
Step 1

SQL ma check karo

StudentProfiles

table ma data che ke nai.

Step 2

Student login kari

report.html

test karo.

Step 3

Admin login kari dropdown ma students load thay che ke nai check karo.

Step 4

Generate Report click kari verify karo.

Step 5

Jo report empty hoy to dummy data insert karo.

InternalMarks
ExternalMarks
Attendance
FeeRecords