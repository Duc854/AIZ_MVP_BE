using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using Shared.Models;
using Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Abstractions
{
    public interface IInterviewSessionService
    {
        Task<Result<string>> StartInterview(UserIdentity identity, int jobDescriptionId);
        Task<Result> EndInterview(UserIdentity identity, Guid interviewSessionId);
        Task<Result<List<InterviewSessionSummaryDto>>> GetUserInterviewHistory(UserIdentity identity);
        Task<Result<InterviewSessionDetailDto>> GetInterviewDetail(Guid sessionId, UserIdentity identity);
    }
}
