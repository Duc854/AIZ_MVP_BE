using System;
using System.Collections.Generic;

namespace AIZ_MVP_Bussiness.Dtos.ResponseDtos
{
    /// <summary>
    /// DTO for interview detail with nested answer and evaluation structure (FE format).
    /// </summary>
    public class InterviewDetailDto
    {
        public Guid SessionId { get; set; }
        public string JobTitle { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public List<TurnDetailDto> Turns { get; set; } = new();
    }

    /// <summary>
    /// DTO for turn detail with nested answer and evaluation.
    /// </summary>
    public class TurnDetailDto
    {
        public int TurnIndex { get; set; }
        public string QuestionText { get; set; } = null!;
        public string? Topic { get; set; }
        public string Difficulty { get; set; } = null!;
        public AnswerDto? Answer { get; set; }
        public EvaluationDto? Evaluation { get; set; }
    }

    /// <summary>
    /// DTO for answer nested in turn.
    /// </summary>
    public class AnswerDto
    {
        public string UserAnswer { get; set; } = null!;
    }

    /// <summary>
    /// DTO for evaluation nested in turn.
    /// </summary>
    public class EvaluationDto
    {
        public string? Result { get; set; } // PASS or FAIL
        public string? Feedback { get; set; }
        public decimal? Score { get; set; }
    }
}
