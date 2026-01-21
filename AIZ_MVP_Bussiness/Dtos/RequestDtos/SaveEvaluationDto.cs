using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    public class SaveEvaluationDto
    {
        public Guid InterviewSessionId { get; set; }
        public int TurnIndex { get; set; }
        public decimal Score { get; set; }
        public string Feedback { get; set; } = null!;
    }
}
