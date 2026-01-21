using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using Shared.Models;
using Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Abstractions
{
    public interface IInterviewEvaluationService
    {
        Task<Result> SaveEvaluation(SaveEvaluationDto dto, UserIdentity identity);
    }
}
