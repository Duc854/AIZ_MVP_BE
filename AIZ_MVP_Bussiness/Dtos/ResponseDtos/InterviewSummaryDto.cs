using System;
using System.Collections.Generic;

namespace AIZ_MVP_Bussiness.Dtos.ResponseDtos
{
    /// <summary>
    /// DTO for interview summary with aggregated statistics and detailed evaluations.
    /// </summary>
    public class InterviewSummaryDto
    {
        public Guid SessionId { get; set; }
        public string JobTitle { get; set; } = null!;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        // Additional fields for FE compatibility
        public int PassedCount => CorrectAnswers;
        public int FailedCount => WrongAnswers;
        public decimal? OverallScore { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public List<TurnEvaluationDto> Evaluations { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual turn evaluation details.
    /// </summary>
    public class TurnEvaluationDto
    {
        public int TurnIndex { get; set; }
        public string Question { get; set; } = null!;
        public string? Topic { get; set; }
        public string Difficulty { get; set; } = null!;
        public string? Answer { get; set; }
        public string? Result { get; set; } // PASS or FAIL
        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
    }
}
