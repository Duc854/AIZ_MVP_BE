using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Context;
using AIZ_MVP_Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Repositories
{
    public class InterviewSessionRepository : IInterviewSessionRepository
    {
        private readonly AIZMvpDbContext _dbContext;
        public InterviewSessionRepository(AIZMvpDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void Add(InterviewSession interviewSession)
        {
            _dbContext.InterviewSessions.Add(interviewSession);
        }

        public async Task<InterviewSession?> GetInterviewSessionToUpdate(Guid interviewSessionId)
        {
            return await _dbContext.InterviewSessions.FirstOrDefaultAsync(i => i.Id == interviewSessionId);
        }

        public async Task<List<InterviewSession>> GetInterviewSessionsByUserId(Guid userId)
        {
            return await _dbContext.InterviewSessions.AsNoTracking().Where(i => i.UserId == userId).ToListAsync();
        }

        public async Task<InterviewSession?> GetSessionWithFullDepth(Guid sessionId)
        {
            return await _dbContext.InterviewSessions
                .Include(s => s.JobDescription)
                .Include(s => s.Turns)
                    .ThenInclude(t => t.Answer)
                        .ThenInclude(a => a.Evaluation)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }
    }
}
