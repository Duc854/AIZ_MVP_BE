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

        public async Task<Result<Guid>> SaveInterviewTurn(SaveInterviewTurnDto dto, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result<Guid>.Fail(
                    new Error("UNAUTHORIZED_USER", "Invalid user identity"));
            }
            var session = await _interviewSessionRepository.GetInterviewSessionToUpdate(dto.InterviewSessionId);
            if (session == null)
            {
                return Result<Guid>.Fail(
                    new Error("SESSION_NOT_FOUND", "Interview session not found")
                );
            }
            if (session.Status != "InProgress")
            {
                return Result<Guid>.Fail(
                    new Error("INTERVIEW_SESSION_CLOSED", "This interview session is closed and cannot accept further responses")
                );
            }
            if (session.UserId != userId)
            {
                return Result<Guid>.Fail(
                    new Error("FORBIDDEN", "You do not own this interview session"));
            }

            // Idempotent behavior: Check if turn already exists for this session+turnIndex
            var existingTurn = await _interviewTurnRepository.GetBySessionAndTurn(dto.InterviewSessionId, dto.TurnIndex);
            if (existingTurn != null)
            {
                // Turn already exists - return existing turn ID (idempotent)
                return Result<Guid>.Success(existingTurn.Id);
            }

            // Validate turnIndex sequence only if creating new turn
            if (dto.TurnIndex != session.CurrentTurnIndex + 1)
            {
                // Check if there's a turn for the expected next index (might be a retry scenario)
                var expectedTurn = await _interviewTurnRepository.GetBySessionAndTurn(dto.InterviewSessionId, session.CurrentTurnIndex + 1);
                if (expectedTurn != null)
                {
                    // Return the expected turn ID instead of error (helps with retry scenarios)
                    return Result<Guid>.Success(expectedTurn.Id);
                }

                // If no expected turn exists, return error (genuine mismatch)
                return Result<Guid>.Fail(
                    new Error("WRONG_TURN", $"Turn index {dto.TurnIndex} does not match expected next turn index {session.CurrentTurnIndex + 1}"));
            }

            // Create new turn
            session.CurrentTurnIndex = dto.TurnIndex;
            var interviewTurn = new InterviewTurn
            {
                Id = Guid.NewGuid(),
                InterviewSessionId = dto.InterviewSessionId,
                QuestionId = dto.QuestionId,
                QuestionContent = dto.ResolvedQuestionText, // Use resolved value (supports both QuestionText and QuestionContent)
                Topic = dto.Topic,
                TurnIndex = dto.TurnIndex,
                Difficulty = dto.Difficulty,
            };
            _interviewTurnRepository.Add(interviewTurn);
            try
            {
                await _uow.SaveChangesAsync();
                return Result<Guid>.Success(interviewTurn.Id);
            }
            catch (DbUpdateException ex)
            {
                // If unique constraint violation (turn already exists), try to get existing turn
                var innerException = ex.InnerException;
                if (innerException != null && innerException.Message.Contains("UNIQUE") || innerException.Message.Contains("duplicate"))
                {
                    var existingTurnAfterError = await _interviewTurnRepository.GetBySessionAndTurn(dto.InterviewSessionId, dto.TurnIndex);
                    if (existingTurnAfterError != null)
                    {
                        // Return existing turn ID (idempotent behavior)
                        return Result<Guid>.Success(existingTurnAfterError.Id);
                    }
                }

                // Log the actual SQL exception for debugging
                var errorMessage = "Cannot create interview turn";
                if (innerException != null)
                {
                    errorMessage = $"{errorMessage}. Details: {innerException.Message}";
                }
                
                return Result<Guid>.Fail(
                    new Error("DB_ERROR", errorMessage)
                );
            }
        }

        public async Task<Result<Guid>> GetTurnIdBySessionAndIndex(Guid sessionId, int turnIndex, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result<Guid>.Fail(
                    new Error("UNAUTHORIZED_USER", "Invalid user identity"));
            }

            var session = await _interviewSessionRepository.GetInterviewSessionToUpdate(sessionId);
            if (session == null)
            {
                return Result<Guid>.Fail(
                    new Error("SESSION_NOT_FOUND", "Interview session not found"));
            }

            if (session.UserId != userId)
            {
                return Result<Guid>.Fail(
                    new Error("FORBIDDEN", "You do not own this interview session"));
            }

            var turn = await _interviewTurnRepository.GetBySessionAndTurn(sessionId, turnIndex);
            if (turn == null)
            {
                return Result<Guid>.Fail(
                    new Error("TURN_NOT_FOUND", $"Turn with index {turnIndex} not found for this session"));
            }

            return Result<Guid>.Success(turn.Id);
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
