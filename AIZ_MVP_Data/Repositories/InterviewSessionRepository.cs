using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Context;
using AIZ_MVP_Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<InterviewSessionRepository>? _logger;

        public InterviewSessionRepository(AIZMvpDbContext dbContext, ILogger<InterviewSessionRepository>? logger = null)
        {
            _dbContext = dbContext;
            _logger = logger;
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
            return await _dbContext.InterviewSessions
                .AsNoTracking()
                .Include(s => s.JobDescription)
                .Where(i => i.UserId == userId)
                .ToListAsync();
        }

        public async Task<InterviewSession?> GetSessionWithFullDepth(Guid sessionId)
        {
            // Try to load with Evaluation first (if schema is up-to-date)
            try
            {
                var session = await _dbContext.InterviewSessions
                    .Include(s => s.JobDescription)
                    .Include(s => s.Turns)
                        .ThenInclude(t => t.Answer)
                            .ThenInclude(a => a.Evaluation)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);
                
                // Log counts for debugging
                if (session != null)
                {
                    var turnsCount = session.Turns?.Count ?? 0;
                    var answersCount = session.Turns?.Count(t => t.Answer != null) ?? 0;
                    var evaluationsCount = session.Turns?.Count(t => t.Answer?.Evaluation != null) ?? 0;
                    
                    _logger?.LogInformation(
                        "[GetSessionWithFullDepth] Loaded session. SessionId: {SessionId}, Turns: {TurnsCount}, Answers: {AnswersCount}, Evaluations: {EvaluationsCount}",
                        sessionId, turnsCount, answersCount, evaluationsCount);
                }
                
                return session;
            }
            catch (SqlException sqlEx) when (
                sqlEx.Number == 207 || // Invalid column name
                sqlEx.Message.Contains("Invalid column name") ||
                sqlEx.Message.Contains("'Result'") ||
                sqlEx.Message.Contains("'CreatedAt'") ||
                sqlEx.Message.Contains("'UpdatedAt'"))
            {
                // Schema mismatch: Load without Evaluation to avoid SQL exception
                _logger?.LogWarning(
                    "[GetSessionWithFullDepth] Schema mismatch detected (missing Evaluation columns). Loading session without Evaluation. SessionId: {SessionId}, Error: {Error}, ErrorNumber: {ErrorNumber}",
                    sessionId, sqlEx.Message, sqlEx.Number);

                // Fallback: Load without Evaluation (safe projection)
                var session = await _dbContext.InterviewSessions
                    .Include(s => s.JobDescription)
                    .Include(s => s.Turns)
                        .ThenInclude(t => t.Answer)
                    // Note: Evaluation is not included to avoid SQL exception
                    .FirstOrDefaultAsync(s => s.Id == sessionId);
                
                if (session != null)
                {
                    var turnsCount = session.Turns?.Count ?? 0;
                    var answersCount = session.Turns?.Count(t => t.Answer != null) ?? 0;
                    _logger?.LogInformation(
                        "[GetSessionWithFullDepth] Loaded session without Evaluation. SessionId: {SessionId}, Turns: {TurnsCount}, Answers: {AnswersCount}",
                        sessionId, turnsCount, answersCount);
                }
                
                return session;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[GetSessionWithFullDepth] Unexpected error loading session. SessionId: {SessionId}", sessionId);
                throw; // Re-throw for service layer to handle
            }
        }
    }
}
