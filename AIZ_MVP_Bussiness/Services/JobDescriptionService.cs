using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Entities;
using Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Services
{
    public class JobDescriptionService : IJobDescriptionService
    {
        private readonly IJobDescriptionRepository _jobDescriptionRepository;
        public JobDescriptionService(IJobDescriptionRepository jobDescriptionRepository)
        {
            _jobDescriptionRepository = jobDescriptionRepository;
        }

        public async Task<Result<List<JobDescription>>> GetAllJobDescription()
        {
            var result = await _jobDescriptionRepository.GetAllJobDescriptionsAsync();
            return Result<List<JobDescription>>.Success(result);
        }
    }
}
