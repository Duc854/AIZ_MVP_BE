using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Abstractions
{
    public interface IInterviewEvaluationRepository
    {
        void Add(InterviewEvaluation interviewEvaluation);
        Task<InterviewEvaluation?> GetEvaluationByAnswerId(Guid interviewAnswerId);
    }
}
