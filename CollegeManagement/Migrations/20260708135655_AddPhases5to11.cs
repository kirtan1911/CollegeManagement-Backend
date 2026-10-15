using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CollegeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddPhases5to11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomeTown",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ResidenceType",
                table: "StudentProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "TransportOpted",
                table: "StudentProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TransportRoute",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EligibilityCriteria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageLPA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DriveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "GradeScales",
                columns: table => new
                {
                    GradeScaleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradePoint = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeScales", x => x.GradeScaleId);
                });

            migrationBuilder.CreateTable(
                name: "Hostels",
                columns: table => new
                {
                    HostelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalRooms = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hostels", x => x.HostelId);
                });

            migrationBuilder.CreateTable(
                name: "RecheckRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentProfileId = table.Column<int>(type: "int", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProcessedByUserId = table.Column<int>(type: "int", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecheckRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_RecheckRequests_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "ExamId");
                    table.ForeignKey(
                        name: "FK_RecheckRequests_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentProfileId");
                });

            migrationBuilder.CreateTable(
                name: "TimeTableEntries",
                columns: table => new
                {
                    EntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Semester = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacultyUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTableEntries", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_TimeTableEntries_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId");
                    table.ForeignKey(
                        name: "FK_TimeTableEntries_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                });

            migrationBuilder.CreateTable(
                name: "PlacementApplications",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentProfileId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementApplications", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_PlacementApplications_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_PlacementApplications_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentProfileId");
                });

            migrationBuilder.CreateTable(
                name: "PlacementRecords",
                columns: table => new
                {
                    PlacementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentProfileId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PackageOffered = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlacedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JobRole = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementRecords", x => x.PlacementId);
                    table.ForeignKey(
                        name: "FK_PlacementRecords_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_PlacementRecords_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentProfileId");
                });

            migrationBuilder.CreateTable(
                name: "HostelRooms",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostelId = table.Column<int>(type: "int", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    OccupiedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostelRooms", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_HostelRooms_Hostels_HostelId",
                        column: x => x.HostelId,
                        principalTable: "Hostels",
                        principalColumn: "HostelId");
                });

            migrationBuilder.CreateTable(
                name: "HostelAllotments",
                columns: table => new
                {
                    AllotmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentProfileId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    AllotmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VacatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FeeStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostelAllotments", x => x.AllotmentId);
                    table.ForeignKey(
                        name: "FK_HostelAllotments_HostelRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "HostelRooms",
                        principalColumn: "RoomId");
                    table.ForeignKey(
                        name: "FK_HostelAllotments_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentProfileId");
                });

            migrationBuilder.InsertData(
                table: "GradeScales",
                columns: new[] { "GradeScaleId", "Grade", "GradePoint", "MaxMarks", "MinMarks" },
                values: new object[,]
                {
                    { 1, "A+", 10m, 100m, 90m },
                    { 2, "A", 9m, 89m, 80m },
                    { 3, "B+", 8m, 79m, 70m },
                    { 4, "B", 7m, 69m, 60m },
                    { 5, "C+", 6m, 59m, 50m },
                    { 6, "C", 5m, 49m, 40m },
                    { 7, "Fail", 0m, 39m, 0m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_HostelAllotments_RoomId",
                table: "HostelAllotments",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelAllotments_StudentProfileId",
                table: "HostelAllotments",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelRooms_HostelId",
                table: "HostelRooms",
                column: "HostelId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementApplications_CompanyId",
                table: "PlacementApplications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementApplications_StudentProfileId",
                table: "PlacementApplications",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementRecords_CompanyId",
                table: "PlacementRecords",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementRecords_StudentProfileId",
                table: "PlacementRecords",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RecheckRequests_ExamId",
                table: "RecheckRequests",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_RecheckRequests_StudentProfileId",
                table: "RecheckRequests",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTableEntries_CourseId",
                table: "TimeTableEntries",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTableEntries_DepartmentId",
                table: "TimeTableEntries",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GradeScales");

            migrationBuilder.DropTable(
                name: "HostelAllotments");

            migrationBuilder.DropTable(
                name: "PlacementApplications");

            migrationBuilder.DropTable(
                name: "PlacementRecords");

            migrationBuilder.DropTable(
                name: "RecheckRequests");

            migrationBuilder.DropTable(
                name: "TimeTableEntries");

            migrationBuilder.DropTable(
                name: "HostelRooms");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Hostels");

            migrationBuilder.DropColumn(
                name: "HomeTown",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "ResidenceType",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "TransportOpted",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "TransportRoute",
                table: "StudentProfiles");
        }
    }
}
