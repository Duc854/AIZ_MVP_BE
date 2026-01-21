using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
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
    public class InterviewSessionService : IInterviewSessionService
    {
        private readonly IInterviewSessionRepository _interviewSessionRepository;
        private readonly ILicenseRepository _licenseRepository;
        private readonly IUnitOfWork _uow;
        public InterviewSessionService(IInterviewSessionRepository interviewSessionRepository, ILicenseRepository licenseRepository, IUnitOfWork uow)
        {
            _interviewSessionRepository = interviewSessionRepository;
            _licenseRepository = licenseRepository;
            _uow = uow;
        }

        //Start interview
        public async Task<Result<string>> StartInterview(
            UserIdentity identity,
            int jobDescriptionId)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result<string>.Fail(
                    new Error("UNAUTHORIZED_USER", "Invalid user identity"));
            }
            var hasValidLicense = await _licenseRepository.HasValidLicenseAsync(userId);
            if (!hasValidLicense)
            {
                return Result<string>.Fail(
                    new Error("LICENSE_INVALID", "License is expired or inactive"));
            }

            var session = new InterviewSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                JobDescriptionId = jobDescriptionId,
            };
            _interviewSessionRepository.Add(session);
            try
            {
                await _uow.SaveChangesAsync();
                return Result<string>.Success(session.Id.ToString());
            }
            catch (DbUpdateException)
            {
                return Result<string>.Fail(
                    new Error("DB_ERROR", "Cannot create interview session")
                );
            }
        }

        //End interview
        public async Task<Result> EndInterview(
            UserIdentity identity,
            Guid interviewSessionId)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result.Fail(
                    new Error("UNAUTHORIZED_USER", "Invalid user identity"));
            }

            var session = await _interviewSessionRepository
                .GetInterviewSessionToUpdate(interviewSessionId);

            if (session == null)
            {
                return Result.Fail(
                    new Error("SESSION_NOT_FOUND", "Interview session not found"));
            }

            if (session.UserId != userId)
            {
                return Result.Fail(
                    new Error("FORBIDDEN", "You do not own this interview session"));
            }

            if (session.Status != "InProgress")
            {
                return Result.Fail(
                    new Error("SESSION_ALREADY_ENDED", "Interview session already ended"));
            }

            session.Status = "Completed";
            session.EndedAt = DateTime.UtcNow;

            try
            {
                await _uow.SaveChangesAsync();
                return Result.Success();
            }
            catch (DbUpdateException)
            {
                return Result.Fail(
                    new Error("DB_ERROR", "Cannot end interview session"));
            }
        }

        public async Task<Result<List<InterviewSessionSummaryDto>>> GetUserInterviewHistory(UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result<List<InterviewSessionSummaryDto>>.Fail(new Error("UNAUTHORIZED", "Invalid user"));
            }
            var sessions = await _interviewSessionRepository.GetInterviewSessionsByUserId(userId);

            var result = sessions.Select(s => new InterviewSessionSummaryDto
            {
                SessionId = s.Id,
                JobTitle = $"{s.JobDescription?.Level} {s.JobDescription?.Sector}" ?? "Unknown Job",
                Status = s.Status,
                TotalTurns = s.CurrentTurnIndex,
                StartedAt = s.StartedAt,
                EndedAt = s.EndedAt!.Value
            }).OrderByDescending(x => x.StartedAt).ToList();

            return Result<List<InterviewSessionSummaryDto>>.Success(result);
        }

        public async Task<Result<InterviewSessionDetailDto>> GetInterviewDetail(Guid sessionId, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result<InterviewSessionDetailDto>.Fail(new Error("UNAUTHORIZED", "Invalid user"));
            }
            var session = await _interviewSessionRepository.GetSessionWithFullDepth(sessionId);

            if (session == null)
                return Result<InterviewSessionDetailDto>.Fail(new Error("NOT_FOUND", "Session not found"));

            if (session.UserId != userId)
                return Result<InterviewSessionDetailDto>.Fail(new Error("FORBIDDEN", "You don't own this session"));

            var detail = new InterviewSessionDetailDto
            {
                SessionId = session.Id,
                JobTitle = $"{session.JobDescription?.Level} {session.JobDescription?.Sector}" ?? "Unknown Job",
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                Turns = session.Turns.OrderBy(t => t.TurnIndex).Select(t => new TurnDetailDto
                {
                    TurnIndex = t.TurnIndex,
                    Question = t.QuestionContent,
                    Difficulty = t.Difficulty,
                    Answer = t.Answer?.AnswerText,
                    Score = t.Answer?.Evaluation?.Score,
                    Feedback = t.Answer?.Evaluation?.Feedback
                }).ToList()
            };

            return Result<InterviewSessionDetailDto>.Success(detail);
        }
    }
}
