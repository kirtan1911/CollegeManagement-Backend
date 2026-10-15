using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollegeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyMaterialCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "StudyMaterials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExternalLink",
                table: "StudyMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PracticalNumber",
                table: "StudyMaterials",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "ExternalLink",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "PracticalNumber",
                table: "StudyMaterials");
        }
    }
}
