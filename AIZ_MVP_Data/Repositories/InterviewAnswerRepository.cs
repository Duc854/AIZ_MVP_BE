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
    public class InterviewAnswerRepository : IInterviewAnswerRepository
    {
        private readonly AIZMvpDbContext _dbContext;
        public InterviewAnswerRepository(AIZMvpDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(InterviewAnswer interviewAnswer)
        {
            _dbContext.InterviewAnswers.Add(interviewAnswer);
        }

        public async Task<InterviewAnswer?> GetAnswerBySessionAndTurn(Guid sessionId, int turnIndex)
        {
            return await _dbContext.InterviewAnswers
                .Include(a => a.InterviewTurn)
                    .ThenInclude(t => t.InterviewSession)
                .FirstOrDefaultAsync(a => a.InterviewTurn.InterviewSessionId == sessionId
                                       && a.InterviewTurn.TurnIndex == turnIndex);
        }

        public async Task<InterviewAnswer?> GetAnswerByTurnIdForUpdate(Guid interviewTurnId)
        {
            // Get answer by turn ID without AsNoTracking() so it can be updated
            return await _dbContext.InterviewAnswers
                .Include(a => a.InterviewTurn)
                    .ThenInclude(t => t.InterviewSession)
                .FirstOrDefaultAsync(a => a.InterviewTurnId == interviewTurnId);
        }

        public async Task<InterviewAnswer?> GetAnswerById(Guid answerId)
        {
            return await _dbContext.InterviewAnswers
                .Include(a => a.InterviewTurn)
                    .ThenInclude(t => t.InterviewSession)
                .FirstOrDefaultAsync(a => a.Id == answerId);
        }
    }
}
