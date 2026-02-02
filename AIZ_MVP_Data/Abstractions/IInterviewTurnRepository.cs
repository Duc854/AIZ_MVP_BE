using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Abstractions
{
    public interface IInterviewTurnRepository
    {
        void Add(InterviewTurn interviewTurn);
        Task<InterviewTurn?> GetBySessionAndTurn(Guid interviewSessionId, int turnIndex);
        Task<InterviewTurn?> GetBySessionAndTurnForUpdate(Guid interviewSessionId, int turnIndex);
    }
}
