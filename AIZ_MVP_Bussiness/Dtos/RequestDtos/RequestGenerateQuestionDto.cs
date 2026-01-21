using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    public class RequestGenerateQuestionDto
    {
        [Required]
        public Guid InterviewSessionId { get; set; }
        [Required]
        public int JobDescriptionId { get; set; }
        [Required]
        public int TurnIndex { get; set; }
    }
}
