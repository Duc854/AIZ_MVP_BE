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
        public async Task<Result> SaveAnswer(SaveInterviewAnswerDto dto, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
                return Result.Fail(new Error("UNAUTHORIZED", "Invalid user"));
            var turn = await _interviewTurnRepository.GetBySessionAndTurn(dto.InterviewSessionId, dto.TurnIndex);

            if (turn == null)
                return Result.Fail(new Error("TURN_NOT_FOUND", "Cannot find this interview turn"));
            if (turn.InterviewSession.UserId != userId)
                return Result.Fail(new Error("FORBIDDEN", "You do not own this interview session"));
            var answer = new InterviewAnswer
            {
                Id = Guid.NewGuid(),
                InterviewTurnId = turn.Id,
                AnswerText = dto.AnswerText
            };

            _interviewAnswerRepository.Add(answer);
            turn.Status = "Completed";

            try
            {
                await _uow.SaveChangesAsync();
                return Result.Success();
            }
            catch (DbUpdateException)
            {
                return Result.Fail(new Error("DB_ERROR", "Cannot create interview answer"));
            }
        }
    }
}
