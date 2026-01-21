using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Context;
using AIZ_MVP_Data.Entities;
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
    }
}
