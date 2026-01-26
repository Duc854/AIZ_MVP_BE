using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIZ_MVP_Data.Migrations
{
    /// <inheritdoc />
    public partial class FixInterviewEvaluationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add Result column if it doesn't exist
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

            // 2. Make Score column nullable if it's currently NOT NULL
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'InterviewEvaluations' 
                    AND COLUMN_NAME = 'Score'
                    AND IS_NULLABLE = 'NO'
                )
                BEGIN
                    -- Change Score from NOT NULL to NULLABLE
                    ALTER TABLE InterviewEvaluations
                    ALTER COLUMN Score DECIMAL(5,2) NULL;
                END
            ");

            // 3. Update Score precision to match entity configuration (5,2) if different
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'InterviewEvaluations' 
                    AND COLUMN_NAME = 'Score'
                    AND (NUMERIC_PRECISION != 5 OR NUMERIC_SCALE != 2)
                )
                BEGIN
                    -- Update existing NULL values temporarily
                    UPDATE InterviewEvaluations
                    SET Score = 0
                    WHERE Score IS NULL;
                    
                    -- Change precision
                    ALTER TABLE InterviewEvaluations
                    ALTER COLUMN Score DECIMAL(5,2) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: Make Score NOT NULL (with default 0)
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'InterviewEvaluations' 
                    AND COLUMN_NAME = 'Score'
                    AND IS_NULLABLE = 'YES'
                )
                BEGIN
                    -- Set NULL values to 0
                    UPDATE InterviewEvaluations
                    SET Score = 0
                    WHERE Score IS NULL;
                    
                    -- Make NOT NULL
                    ALTER TABLE InterviewEvaluations
                    ALTER COLUMN Score DECIMAL(5,2) NOT NULL;
                END
            ");

            // Note: We don't drop Result column in Down() to preserve data
            // If needed, it can be dropped manually
        }
    }
}
