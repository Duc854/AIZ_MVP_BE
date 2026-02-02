using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    /// <summary>
    /// DTO for saving interview turn. Supports both 'questionText' and 'questionContent' field names for backward compatibility.
    /// </summary>
    public class SaveInterviewTurnDto
    {
        [Required(ErrorMessage = "InterviewSessionId is required")]
        public Guid InterviewSessionId { get; set; }

        [Required(ErrorMessage = "TurnIndex is required")]
        [Range(1, int.MaxValue, ErrorMessage = "TurnIndex must be greater than 0")]
        public int TurnIndex { get; set; }

        public string? QuestionId { get; set; }

        /// <summary>
        /// Question text content (preferred field name).
        /// </summary>
        [JsonPropertyName("questionText")]
        public string? QuestionText { get; set; }

        /// <summary>
        /// Legacy field name for question content. Maps to QuestionText if QuestionText is not provided.
        /// </summary>
        [JsonPropertyName("questionContent")]
        public string? QuestionContent { get; set; }

        public string? Topic { get; set; }

        [Required(ErrorMessage = "Difficulty is required")]
        [MinLength(1, ErrorMessage = "Difficulty cannot be empty")]
        public string Difficulty { get; set; } = null!;

        /// <summary>
        /// Gets the resolved question text, using QuestionText if available, otherwise QuestionContent.
        /// This ensures backward compatibility with FE sending 'questionContent'.
        /// </summary>
        [JsonIgnore]
        public string ResolvedQuestionText
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(QuestionText))
                    return QuestionText;
                if (!string.IsNullOrWhiteSpace(QuestionContent))
                    return QuestionContent;
                return string.Empty;
            }
        }
    }
}
