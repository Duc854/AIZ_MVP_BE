using AIZ_MVP_Bussiness.Dtos.RequestDtos;
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
    public interface IInterviewTurnService
    {
        //Src cho truong hop su dung BE call API AI de gen question
        //Task<Result<QuestionResponseDto>> GenerateQuestion(RequestGenerateQuestionDto dto, UserIdentity identity);

        Task<Result> SaveInterviewTurn(SaveInterviewTurnDto dto, UserIdentity identity);
    }
}
