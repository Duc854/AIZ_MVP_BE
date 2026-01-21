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
    public class InterviewTurnRepository : IInterviewTurnRepository
    {
        private readonly AIZMvpDbContext _dbContext;
        public InterviewTurnRepository(AIZMvpDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<InterviewTurn?> GetBySessionAndTurn(
                Guid interviewSessionId,
                int turnIndex)
        {
            return await _dbContext.InterviewTurns
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.InterviewSessionId == interviewSessionId &&
                    t.TurnIndex == turnIndex);
        }

        public void Add(InterviewTurn interviewTurn)
        {
            _dbContext.Add(interviewTurn);
        }
    }
}
