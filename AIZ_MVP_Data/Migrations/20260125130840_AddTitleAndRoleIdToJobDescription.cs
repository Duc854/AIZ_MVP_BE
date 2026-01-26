using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIZ_MVP_Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleAndRoleIdToJobDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "JobDescriptions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "JobDescriptions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "InterviewTurns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "InterviewEvaluations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "InterviewEvaluations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "InterviewEvaluations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Result",
                table: "InterviewEvaluations");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "InterviewTurns");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "JobDescriptions");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "JobDescriptions");
        }
    }
}
