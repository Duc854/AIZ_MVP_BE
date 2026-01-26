using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIZ_MVP_Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResultColumnToInterviewEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safe migration: Add Result column only if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'InterviewEvaluations' 
                    AND COLUMN_NAME = 'Result'
                )
                BEGIN
                    ALTER TABLE InterviewEvaluations
                    ADD Result NVARCHAR(50) NULL;
                    
                    -- Update existing rows with default value
                    UPDATE InterviewEvaluations
                    SET Result = 'PENDING'
                    WHERE Result IS NULL;
                    
                    -- Make column NOT NULL after updating existing rows
                    ALTER TABLE InterviewEvaluations
                    ALTER COLUMN Result NVARCHAR(50) NOT NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Result column if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'InterviewEvaluations' 
                    AND COLUMN_NAME = 'Result'
                )
                BEGIN
                    ALTER TABLE InterviewEvaluations
                    DROP COLUMN Result;
                END
            ");
        }
    }
}
