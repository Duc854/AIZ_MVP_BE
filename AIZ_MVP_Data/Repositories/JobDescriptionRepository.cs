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
    public class JobDescriptionRepository : IJobDescriptionRepository
    {
        private readonly AIZMvpDbContext _dbContext;

        public JobDescriptionRepository(AIZMvpDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<JobDescription>> GetAllJobDescriptionsAsync()
        {
            return await _dbContext.JobDescriptions.ToListAsync();
        }

        public async Task<JobDescription?> GetJobDescriptionByIdAsync(int id)
        {
            return await _dbContext.JobDescriptions.FindAsync(id);
        }
    }
}
