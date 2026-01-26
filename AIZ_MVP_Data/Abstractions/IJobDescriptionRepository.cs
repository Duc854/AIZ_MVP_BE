using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Abstractions
{
    public interface IJobDescriptionRepository
    {
        Task<List<JobDescription>> GetAllJobDescriptionsAsync();
        Task<JobDescription?> GetJobDescriptionByIdAsync(int id);
    }
}
