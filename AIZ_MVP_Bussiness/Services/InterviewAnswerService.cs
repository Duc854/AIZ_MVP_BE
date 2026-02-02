using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Entities;
using AIZ_MVP_Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Services
{
    public class InterviewAnswerService : IInterviewAnswerService
    {
        private readonly IInterviewAnswerRepository _interviewAnswerRepository;
        private readonly IInterviewTurnRepository _interviewTurnRepository;
        private readonly IUnitOfWork _uow;
        public InterviewAnswerService(IInterviewTurnRepository interviewTurnRepository, IInterviewAnswerRepository interviewAnswerRepository, IUnitOfWork uow)
        {
            _interviewAnswerRepository = interviewAnswerRepository;
            _interviewTurnRepository = interviewTurnRepository;
            _uow = uow;
        }
        public async Task<Result<Guid>> SaveAnswer(SaveInterviewAnswerDto dto, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
                return Result<Guid>.Fail(new Error("UNAUTHORIZED", "Invalid user"));

            // Get turn with session for validation (with tracking for update)
            var turn = await _interviewTurnRepository.GetBySessionAndTurnForUpdate(dto.InterviewSessionId, dto.TurnIndex);

            if (turn == null)
                return Result<Guid>.Fail(new Error("TURN_NOT_FOUND", "Cannot find this interview turn"));
            
            if (turn.InterviewSession.UserId != userId)
                return Result<Guid>.Fail(new Error("FORBIDDEN", "You do not own this interview session"));

            // Idempotent behavior: Check if answer already exists for this turn
            var existingAnswer = await _interviewAnswerRepository.GetAnswerByTurnIdForUpdate(turn.Id);

            if (existingAnswer != null)
            {
                // Update existing answer (idempotent)
                existingAnswer.AnswerText = dto.UserAnswer;
                existingAnswer.UpdatedAt = DateTime.UtcNow;
                // Note: We don't need to call Add() since the entity is already tracked
            }
            else
            {
                // Create new answer
                var answer = new InterviewAnswer
                {
                    Id = Guid.NewGuid(),
                    InterviewTurnId = turn.Id,
                    AnswerText = dto.UserAnswer, // Map from DTO
                    CreatedAt = DateTime.UtcNow
                };

                _interviewAnswerRepository.Add(answer);
                existingAnswer = answer; // Use for return value
            }

            turn.Status = "Completed";

            try
            {
                await _uow.SaveChangesAsync();
                return Result<Guid>.Success(existingAnswer.Id);
            }
            catch (DbUpdateException ex)
            {
                // Log the actual SQL exception for debugging
                var innerException = ex.InnerException;
                var errorMessage = "Cannot create interview answer";
                
                if (innerException != null)
                {
                    errorMessage = $"{errorMessage}. Details: {innerException.Message}";
                }
                
                return Result<Guid>.Fail(new Error("DB_ERROR", errorMessage));
            }
            catch (Exception ex)
            {
                // Catch any other exceptions (like NullReferenceException)
                return Result<Guid>.Fail(new Error("INTERNAL_ERROR", $"Unexpected error: {ex.Message}"));
            }
        }
    }
}
