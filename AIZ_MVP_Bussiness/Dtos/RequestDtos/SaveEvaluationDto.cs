using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    public class SaveEvaluationDto
    {
        /// <summary>
        /// Optional: InterviewAnswerId to directly identify the answer.
        /// If provided, this takes precedence over InterviewSessionId + TurnIndex.
        /// </summary>
        public Guid? InterviewAnswerId { get; set; }

        /// <summary>
        /// Required if InterviewAnswerId is not provided.
        /// Used together with TurnIndex to find the answer.
        /// </summary>
        public Guid? InterviewSessionId { get; set; }

        /// <summary>
        /// Required if InterviewAnswerId is not provided.
        /// Used together with InterviewSessionId to find the answer.
        /// </summary>
        public int? TurnIndex { get; set; }

        [Required(ErrorMessage = "Result is required")]
        public string Result { get; set; } = null!; // PASS or FAIL

        [Required(ErrorMessage = "Feedback is required")]
        public string Feedback { get; set; } = null!;

        public string? PhoneticFeedback { get; set; } // Optional

        public decimal? Score { get; set; } // Optional

        /// <summary>
        /// Validates that either InterviewAnswerId is provided (and not empty GUID), or both InterviewSessionId and TurnIndex are provided.
        /// </summary>
        public bool IsValid()
        {
            // Check if InterviewAnswerId is provided and not empty GUID
            if (InterviewAnswerId.HasValue && InterviewAnswerId.Value != Guid.Empty)
            {
                return true;
            }

            // Fallback: Check if both InterviewSessionId and TurnIndex are provided
            return InterviewSessionId.HasValue && TurnIndex.HasValue;
        }

        /// <summary>
        /// Gets a human-readable description of which lookup method will be used.
        /// </summary>
        public string GetLookupMethod()
        {
            if (InterviewAnswerId.HasValue && InterviewAnswerId.Value != Guid.Empty)
            {
                return "InterviewAnswerId";
            }
            if (InterviewSessionId.HasValue && TurnIndex.HasValue)
            {
                return "InterviewSessionId + TurnIndex";
            }
            return "None (Invalid)";
        }
    }
}
