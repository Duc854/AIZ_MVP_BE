using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using AIZ_MVP_Data.Abstractions;
using Shared.Models;
using Shared.Wrappers;
using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIZ_MVP_Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AIZ_MVP_Bussiness.Services
{
    public class InterviewTurnService : IInterviewTurnService
    {
        private readonly IInterviewTurnRepository _interviewTurnRepository;
        private readonly IInterviewSessionRepository _interviewSessionRepository;
        private readonly IUnitOfWork _uow;
        public InterviewTurnService(IInterviewTurnRepository interviewTurnRepository, IInterviewSessionRepository interviewSessionRepository, IUnitOfWork uow)
        {
            _interviewTurnRepository = interviewTurnRepository;
            _interviewSessionRepository = interviewSessionRepository;
            _uow = uow; 
        }

        public async Task<Result> SaveInterviewTurn(SaveInterviewTurnDto dto, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result.Fail(
                    new Error("UNAUTHORIZED_USER", "Invalid user identity"));
            }
            var session = await _interviewSessionRepository.GetInterviewSessionToUpdate(dto.InterviewSessionId);
            if (session == null)
            {
                return Result.Fail(
                    new Error("SESSION_NOT_FOUND", "Interview session not found")
                );
            }
            if (session.Status != "InProgress")
            {
                return Result.Fail(
                    new Error("INTERVIEW_SESSION_CLOSED", "This interview session is closed and cannot accept further responses")
                );
            }
            if (session.UserId != userId)
            {
                return Result.Fail(
                    new Error("FORBIDDEN", "You do not own this interview session"));
            }
            if (dto.TurnIndex != session.CurrentTurnIndex + 1) //Dang su dung FE call thang API AI nen current index se lay tu reponse cua AI nen se lon hon turn truoc (turn dc luu trong DB)
            {
                return Result.Fail(
                    new Error("WRONG_TURN", "Turn index does not match the current session state"));
            }
            session.CurrentTurnIndex = dto.TurnIndex;
            var interviewTurn = new InterviewTurn
            {
                Id = Guid.NewGuid(),
                InterviewSessionId = dto.InterviewSessionId,
                QuestionId = dto.QuestionId,
                QuestionContent = dto.QuestionContent,
                TurnIndex = dto.TurnIndex,
                Difficulty = dto.Difficulty,
            };
            _interviewTurnRepository.Add(interviewTurn);
            try
            {
                await _uow.SaveChangesAsync();
                return Result.Success();
            }
            catch (DbUpdateException)
            {
                return Result.Fail(
                    new Error("DB_ERROR", "Cannot create interview turn")
                );
            }
        }

        //Src code cho truong hop su dung be goi AI api de xay dung interview stateful 
        //public async Task<Result<QuestionResponseDto>> GenerateQuestion(RequestGenerateQuestionDto dto, UserIdentity identity)
        //{
        //    if (!Guid.TryParse(identity.UserId, out var userId))
        //    {
        //        return Result<QuestionResponseDto>.Fail(
        //            new Error("UNAUTHORIZED_USER", "Invalid user identity"));
        //    }
        //    var session = await _interviewSessionRepository.GetInterviewSessionById(dto.InterviewSessionId);
        //    if (session == null)
        //    {
        //        return Result<QuestionResponseDto>.Fail(
        //            new Error("SESSION_NOT_FOUND", "Interview session not found")
        //        );
        //    }
        //    if (session.Status != "InProgress")
        //    {
        //        return Result<QuestionResponseDto>.Fail(
        //            new Error("INTERVIEW_SESSION_CLOSED", "This interview session is closed and cannot accept further responses")
        //        );
        //    }
        //    if (session.UserId != userId)
        //    {
        //        return Result<QuestionResponseDto>.Fail(
        //            new Error("FORBIDDEN", "You do not own this interview session"));
        //    }
        //    if(dto.TurnIndex != session.CurrentTurnIndex)
        //    {
        //        return Result<QuestionResponseDto>.Fail(
        //            new Error("WRONG_TURN", "Turn index does not match the current session state"));
        //    }
        //    if (session.JobDescriptionId != dto.JobDescriptionId)
        //    {
        //        return Result<QuestionResponseDto>.Fail(
        //            new Error("JOB_MISMATCH", "Job description does not belong to this session"));
        //    }
        //    var responseFromAI = new QuestionResponseDto { }; //Giả lập đã call dc background service từ AI gen câu hỏi
        //    if(responseFromAI == null)
        //    {
        //        return Result<QuestionResponseDto>.Fail(
        //            new Error("AI_GENERATION_FAILED", "AI service response fail")
        //        );
        //    }
        //    if (responseFromAI.TurnIndex != dto.TurnIndex)
        //    {
        //        return Result<QuestionResponseDto>.Fail(
        //            new Error("AI_TURN_MISMATCH", "AI response wrong turn")
        //        );
        //    }
        //    var interviewTurn = new InterviewTurn
        //    {
        //        Id = Guid.NewGuid(),
        //        InterviewSessionId = dto.InterviewSessionId,
        //        QuestionId = responseFromAI.QuestionId,
        //        QuestionContent = responseFromAI.Content,
        //        TurnIndex = dto.TurnIndex,
        //        Difficulty = responseFromAI.Difficulty,
        //    };
        //    _interviewTurnRepository.Add(interviewTurn);
        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //        responseFromAI.InterviewTurnId = interviewTurn.Id; 
        //        return Result<QuestionResponseDto>.Success(responseFromAI);
        //    }
        //    catch (DbUpdateException)
        //    {
        //        var existedTurn = await _interviewTurnRepository
        //            .GetBySessionAndTurn(dto.InterviewSessionId, dto.TurnIndex);

        //        if (existedTurn != null)
        //        {
        //            return Result<QuestionResponseDto>.Success(new QuestionResponseDto
        //            {
        //                InterviewTurnId = existedTurn.Id,
        //                TurnIndex = existedTurn.TurnIndex,
        //                QuestionId = existedTurn.QuestionId,
        //                Content = existedTurn.QuestionContent,
        //                Difficulty = existedTurn.Difficulty
        //            });
        //        }

        //        return Result<QuestionResponseDto>.Fail(
        //            new Error("DB_ERROR", "Cannot create interview turn")
        //        );
        //    }
        //}
    }
}
