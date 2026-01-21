using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using AIZ_MVP_Bussiness.Services;
using AIZ_MVP_Data.Entities;
using Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Abstractions
{
    public interface IJobDescriptionService
    {
        Task<Result<List<JobDescription>>> GetAllJobDescription();
    }
}
