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
    public class InterviewEvaluationRepository : IInterviewEvaluationRepository
    {
        private readonly AIZMvpDbContext _dbContext;
        public InterviewEvaluationRepository(AIZMvpDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(InterviewEvaluation interviewAnswer)
        {
            _dbContext.InterviewEvaluations.Add(interviewAnswer);
        }

        public async Task<InterviewEvaluation?> GetEvaluationByAnswerId(Guid interviewAnswerId)
        {
            try
            {
                return await _dbContext.InterviewEvaluations
                    .FirstOrDefaultAsync(e => e.InterviewAnswerId == interviewAnswerId);
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Message.Contains("Invalid column name 'Result'"))
            {
                // Log the error for debugging
                throw new InvalidOperationException(
                    "Database schema mismatch: 'Result' column is missing from InterviewEvaluations table. " +
                    "Please apply the migration 'FixInterviewEvaluationSchema' or 'AddResultColumnToInterviewEvaluations'.", 
                    sqlEx);
            }
        }
    }
}
