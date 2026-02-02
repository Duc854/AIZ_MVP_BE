using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Entities
{
    public class InterviewTurn
    {
        public Guid Id { get; set; }
        public Guid InterviewSessionId { get; set; }
        public InterviewSession InterviewSession { get; set; } = null!;
        public string? QuestionId { get; set; }
        public string QuestionContent { get; set; } = null!;
        public string? Topic { get; set; }
        public string Difficulty { get; set; } = null!;
        public int TurnIndex { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual InterviewAnswer? Answer { get; set; }
    }
}
