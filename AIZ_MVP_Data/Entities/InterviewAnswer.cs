using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Entities
{
    public class InterviewAnswer
    {
        public Guid Id { get; set; }
        public Guid InterviewTurnId { get; set; }
        public InterviewTurn InterviewTurn { get; set; } = null!;
        public string AnswerText { get; set; } = null!;
        
        [NotMapped]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [NotMapped]
        public DateTime? UpdatedAt { get; set; }
        
        public virtual InterviewEvaluation? Evaluation { get; set; }
    }
}
