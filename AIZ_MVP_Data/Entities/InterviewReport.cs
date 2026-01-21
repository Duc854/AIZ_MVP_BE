using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Entities
{
    public class InterviewReport
    {
        public Guid Id { get; set; }

        public Guid InterviewSessionId { get; set; }
        public InterviewSession InterviewSession { get; set; } = null!;

        public decimal OverallScore { get; set; }
        public string Summary { get; set; } = null!;
        public string Strengths { get; set; } = null!;
        public string Weaknesses { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
