using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Entities
{
    public class InterviewSession
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int JobDescriptionId { get; set; }
        public  JobDescription JobDescription { get; set; } = null!;

        public int CurrentTurnIndex { get; set; } = 0;
        public string Status { get; set; } = "InProgress";
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
        public ICollection<InterviewTurn> Turns { get; set; } = new List<InterviewTurn>();
    }
}
