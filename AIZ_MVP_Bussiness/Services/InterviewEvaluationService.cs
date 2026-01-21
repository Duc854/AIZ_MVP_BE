using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Entities;
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
    public class InterviewEvaluationService : IInterviewEvaluationService
    {
        private readonly IInterviewEvaluationRepository _interviewEvaluationRepository;
        private readonly IInterviewAnswerRepository _interviewAnswerRepository;
        private readonly IUnitOfWork _uow;
        public InterviewEvaluationService(IInterviewEvaluationRepository interviewEvaluationRepository, IInterviewAnswerRepository interviewAnswerRepository, IUnitOfWork uow)
        {
            _interviewEvaluationRepository = interviewEvaluationRepository;
            _interviewAnswerRepository = interviewAnswerRepository;
            _uow = uow;
        }
        public async Task<Result> SaveEvaluation(SaveEvaluationDto dto, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
                return Result.Fail(new Error("UNAUTHORIZED", "Invalid user identity"));
            var answer = await _interviewAnswerRepository.GetAnswerBySessionAndTurn(dto.InterviewSessionId, dto.TurnIndex);

            if (answer == null)
            {
                return Result.Fail(new Error("ANSWER_NOT_FOUND", "Cannot find this interview answer"));
            }
            if (answer.InterviewTurn.InterviewSession.UserId != userId)
            {
                return Result.Fail(new Error("FORBIDDEN", "You do not own this interview session"));
            }

            var evaluation = new InterviewEvaluation
            {
                Id = Guid.NewGuid(),
                InterviewAnswerId = answer.Id,
                Score = dto.Score,
                Feedback = dto.Feedback,
                CreatedAt = DateTime.UtcNow
            };
            _interviewEvaluationRepository.Add(evaluation);
            try
            {
                await _uow.SaveChangesAsync();
                return Result.Success();
            }
            catch (DbUpdateException)
            {
                return Result.Fail(new Error("DB_ERROR", "Cannot create interview evaluation"));
            }
        }
    }
}
