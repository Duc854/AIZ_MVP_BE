using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    public class SaveInterviewTurnDto
    {
        public Guid InterviewSessionId { get; set; }
        public string? QuestionId { get; set; }
        public string QuestionContent { get; set; } = null!;
        public string Difficulty { get; set; } = null!;
        public int TurnIndex { get; set; }
    }
}
