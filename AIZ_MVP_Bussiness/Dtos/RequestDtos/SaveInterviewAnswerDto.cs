using System;
using System.ComponentModel.DataAnnotations;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    /// <summary>
    /// DTO for saving interview answer. Uses TurnIndex to find the turn (since save-turn returns turnId).
    /// </summary>
    public class SaveInterviewAnswerDto
    {
        [Required(ErrorMessage = "InterviewSessionId is required")]
        public Guid InterviewSessionId { get; set; }

        [Required(ErrorMessage = "TurnIndex is required")]
        [Range(1, int.MaxValue, ErrorMessage = "TurnIndex must be greater than 0")]
        public int TurnIndex { get; set; }

        [Required(ErrorMessage = "UserAnswer is required")]
        [MinLength(1, ErrorMessage = "UserAnswer cannot be empty")]
        public string UserAnswer { get; set; } = null!; // Maps to AnswerText in entity
    }
}
