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
        public DateTime EndedAt { get; set; }
    }

    public class InterviewSessionDetailDto
    {
        public Guid SessionId { get; set; }
        public string JobTitle { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public List<TurnDetailDto> Turns { get; set; } = new();
    }

    public class TurnDetailDto
    {
        public int TurnIndex { get; set; }
        public string Question { get; set; } = null!;
        public string Difficulty { get; set; } = null!;
        public string? Answer { get; set; }
        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
    }
}
