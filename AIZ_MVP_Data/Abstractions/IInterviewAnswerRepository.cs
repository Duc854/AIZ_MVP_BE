using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Abstractions
{
    public interface IInterviewAnswerRepository
    {
        void Add(InterviewAnswer interviewAnswer);
        Task<InterviewAnswer?> GetAnswerBySessionAndTurn(Guid sessionId, int turnIndex);
        Task<InterviewAnswer?> GetAnswerByTurnIdForUpdate(Guid interviewTurnId);
        Task<InterviewAnswer?> GetAnswerById(Guid answerId);
    }
}
