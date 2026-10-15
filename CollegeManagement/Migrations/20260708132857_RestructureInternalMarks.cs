using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollegeManagement.Migrations
{
    /// <inheritdoc />
    public partial class RestructureInternalMarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LectureNoteMarks",
                table: "InternalMarks");

            migrationBuilder.RenameColumn(
                name: "VivaMarks",
                table: "InternalMarks",
                newName: "FacultyDiscretionMarks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FacultyDiscretionMarks",
                table: "InternalMarks",
                newName: "VivaMarks");

            migrationBuilder.AddColumn<decimal>(
                name: "LectureNoteMarks",
                table: "InternalMarks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
