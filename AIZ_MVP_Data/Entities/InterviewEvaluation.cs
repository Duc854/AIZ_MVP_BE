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
        public string Result { get; set; } = null!; // PASS or FAIL
        public decimal? Score { get; set; } // Optional
        public string Feedback { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
