using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Abstractions
{
    public interface IInterviewSessionRepository
    {
        void Add(InterviewSession interviewSession);
        Task<InterviewSession?> GetInterviewSessionToUpdate(Guid interviewSessionId);
        Task<List<InterviewSession>> GetInterviewSessionsByUserId(Guid userId);
        Task<InterviewSession?> GetSessionWithFullDepth(Guid sessionId);
    }
}
