using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.ResponseDtos
{
    public class QuestionResponseDto
    {
        public int TurnIndex { get; set; }
        public Guid InterviewTurnId { get; set; }
        public string? QuestionId { get; set; }
        public string Content { get; set; } = null!;
        public string Difficulty { get; set; } = null!;
    }
}
