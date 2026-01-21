using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Entities
{
    public class InterviewEvaluation
    {
        public Guid Id { get; set; }
        public Guid InterviewAnswerId { get; set; }
        public InterviewAnswer InterviewAnswer { get; set; } = null!;
        public decimal Score { get; set; }
        public string Feedback { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
