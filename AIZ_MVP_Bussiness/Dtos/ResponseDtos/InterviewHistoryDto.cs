using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.ResponseDtos
{
    public class InterviewSessionSummaryDto
    {
        public Guid SessionId { get; set; }
        public string JobTitle { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int TotalTurns { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public decimal? OverallScore { get; set; } // Added for Dashboard
    }

    // Note: InterviewSessionDetailDto and TurnDetailDto moved to InterviewDetailDto.cs
    // to avoid duplicate definition and use nested structure (FE format)
}
