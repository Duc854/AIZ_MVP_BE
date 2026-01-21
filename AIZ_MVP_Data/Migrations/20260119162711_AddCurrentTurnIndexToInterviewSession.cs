using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIZ_MVP_Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentTurnIndexToInterviewSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentTurnIndex",
                table: "InterviewSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentTurnIndex",
                table: "InterviewSessions");
        }
    }
}
