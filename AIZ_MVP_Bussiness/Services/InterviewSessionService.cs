using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Entities;
using AIZ_MVP_Data.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly IJobDescriptionRepository _jobDescriptionRepository;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<InterviewSessionService>? _logger;

        public InterviewSessionService(
            IInterviewSessionRepository interviewSessionRepository, 
            ILicenseRepository licenseRepository, 
            IJobDescriptionRepository jobDescriptionRepository, 
            IUnitOfWork uow,
            ILogger<InterviewSessionService>? logger = null)
        {
            _interviewSessionRepository = interviewSessionRepository;
            _licenseRepository = licenseRepository;
            _jobDescriptionRepository = jobDescriptionRepository;
            _uow = uow;
            _logger = logger;
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

            // Validate JobDescription exists
            var jobDescription = await _jobDescriptionRepository.GetJobDescriptionByIdAsync(jobDescriptionId);
            if (jobDescription == null)
            {
                return Result<string>.Fail(
                    new Error("JOB_DESCRIPTION_NOT_FOUND", "Job description not found"));
            }

            // MVP Mode: Auto-create free license if user doesn't have one
            var hasValidLicense = await _licenseRepository.HasValidLicenseAsync(userId);
            if (!hasValidLicense)
            {
                var existingLicense = await _licenseRepository.GetLicenseByUserIdForUpdate(userId);
                if (existingLicense == null)
                {
                    // Create a free license for MVP
                    var freeLicense = new License
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        LicenseKey = $"FREE-{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        Plan = "Free",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        ExpiredAt = null // Free license never expires in MVP
                    };
                    _licenseRepository.Add(freeLicense);
                }
                else
                {
                    // Reactivate existing license if inactive
                    existingLicense.IsActive = true;
                    existingLicense.ExpiredAt = null;
                }
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

            session.Status = "Ended";
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

            var result = new List<InterviewSessionSummaryDto>();
            
            foreach (var session in sessions)
            {
                // Calculate overallScore safely (handle schema mismatch)
                decimal? overallScore = null;
                try
                {
                    // Try to load session with evaluations to calculate score
                    var sessionWithEvals = await _interviewSessionRepository.GetSessionWithFullDepth(session.Id);
                    if (sessionWithEvals?.Turns != null)
                    {
                        var scores = sessionWithEvals.Turns
                            .Where(t => t.Answer?.Evaluation?.Score.HasValue == true)
                            .Select(t => t.Answer!.Evaluation!.Score!.Value)
                            .ToList();
                        if (scores.Any())
                        {
                            overallScore = scores.Average();
                        }
                    }
                }
                catch (SqlException sqlEx) when (
                    sqlEx.Number == 207 || 
                    sqlEx.Message.Contains("Invalid column name") ||
                    sqlEx.Message.Contains("'Result'"))
                {
                    // Schema mismatch - overallScore remains null
                    _logger?.LogDebug("[GetUserInterviewHistory] Schema mismatch for session {SessionId}, overallScore will be null", session.Id);
                }
                catch (Exception ex)
                {
                    // Log but don't fail - overallScore remains null
                    _logger?.LogWarning(ex, "[GetUserInterviewHistory] Error calculating overallScore for session {SessionId}", session.Id);
                }

                result.Add(new InterviewSessionSummaryDto
                {
                    SessionId = session.Id,
                    JobTitle = session.JobDescription?.Title ?? "Unknown Job",
                    Status = session.Status,
                    TotalTurns = session.CurrentTurnIndex,
                    StartedAt = session.StartedAt,
                    EndedAt = session.EndedAt,
                    OverallScore = overallScore
                });
            }

            return Result<List<InterviewSessionSummaryDto>>.Success(result.OrderByDescending(x => x.StartedAt).ToList());
        }

        public async Task<Result<InterviewSummaryDto>> GetInterviewDetail(Guid sessionId, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result<InterviewSummaryDto>.Fail(new Error("UNAUTHORIZED", "Invalid user"));
            }

            InterviewSession? session = null;
            bool schemaMismatch = false;
            try
            {
                session = await _interviewSessionRepository.GetSessionWithFullDepth(sessionId);
            }
            catch (SqlException sqlEx) when (
                sqlEx.Number == 207 || // Invalid column name
                sqlEx.Message.Contains("Invalid column name") ||
                sqlEx.Message.Contains("'Result'") ||
                sqlEx.Message.Contains("'CreatedAt'") ||
                sqlEx.Message.Contains("'UpdatedAt'"))
            {
                // Repository should have handled this with fallback, but if it still throws,
                // we mark schema mismatch and continue - repository fallback should have loaded session
                _logger?.LogWarning(sqlEx, 
                    "[GetInterviewDetail] Schema mismatch detected. Repository fallback should have handled this. SessionId: {SessionId}", 
                    sessionId);
                
                schemaMismatch = true;
                // Repository's fallback should have already loaded session without Evaluation
                // If session is still null, it means a different error occurred
                if (session == null)
                {
                    _logger?.LogError(sqlEx, 
                        "[GetInterviewDetail] Session is null after schema mismatch. This should not happen if repository fallback works. SessionId: {SessionId}", 
                        sessionId);
                    return Result<InterviewSummaryDto>.Fail(new Error("INTERNAL_ERROR", 
                        "Failed to load interview session. Please check database connection."));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[GetInterviewDetail] Unexpected error loading session. SessionId: {SessionId}", sessionId);
                return Result<InterviewSummaryDto>.Fail(new Error("INTERNAL_ERROR", 
                    $"An error occurred while loading interview session: {ex.Message}"));
            }

            if (session == null)
                return Result<InterviewSummaryDto>.Fail(new Error("NOT_FOUND", "Session not found"));

            if (session.UserId != userId)
                return Result<InterviewSummaryDto>.Fail(new Error("FORBIDDEN", "You don't own this session"));

            // Log loaded data counts for debugging
            var allTurns = session.Turns?.ToList() ?? new List<InterviewTurn>();
            var turnsWithAnswers = allTurns.Where(t => t.Answer != null).ToList();
            var turnsWithEvaluations = allTurns.Where(t => t?.Answer?.Evaluation != null).ToList();
            
            if (schemaMismatch)
            {
                _logger?.LogWarning(
                    "[GetInterviewDetail] Schema mismatch detected. Returning session without evaluations. SessionId: {SessionId}, TotalTurns: {TotalTurns}, TurnsWithAnswers: {TurnsWithAnswers}",
                    sessionId, allTurns.Count, turnsWithAnswers.Count);
            }
            else
            {
                _logger?.LogInformation(
                    "[GetInterviewDetail] Session data loaded. SessionId: {SessionId}, TotalTurns: {TotalTurns}, TurnsWithAnswers: {TurnsWithAnswers}, TurnsWithEvaluations: {TurnsWithEvaluations}",
                    sessionId, allTurns.Count, turnsWithAnswers.Count, turnsWithEvaluations.Count);
            }

            // Calculate statistics from evaluations (defensive null checks)
            var totalQuestions = allTurns.Count;
            
            // Count PASS/FAIL results (case-insensitive, handle null/empty)
            var correctAnswers = turnsWithEvaluations
                .Where(e => e?.Answer?.Evaluation?.Result != null)
                .Count(e => e.Answer!.Evaluation!.Result!.Equals("PASS", StringComparison.OrdinalIgnoreCase));
            var wrongAnswers = turnsWithEvaluations
                .Where(e => e?.Answer?.Evaluation?.Result != null)
                .Count(e => e.Answer!.Evaluation!.Result!.Equals("FAIL", StringComparison.OrdinalIgnoreCase));

            // Calculate overall score (average of all scores that have values)
            var scores = turnsWithEvaluations
                .Where(e => e.Answer?.Evaluation?.Score.HasValue == true)
                .Select(e => e.Answer!.Evaluation!.Score!.Value)
                .ToList();
            var overallScore = scores.Any() ? scores.Average() : (decimal?)null;
            
            // Log detailed evaluation data for debugging
            if (turnsWithEvaluations.Any())
            {
                var evaluationResults = turnsWithEvaluations
                    .Select(e => new { 
                        TurnIndex = e.TurnIndex, 
                        Result = e.Answer?.Evaluation?.Result ?? "NULL",
                        Score = e.Answer?.Evaluation?.Score?.ToString() ?? "NULL"
                    })
                    .ToList();
                _logger?.LogInformation(
                    "[GetInterviewDetail] Evaluation details: {EvaluationDetails}",
                    string.Join(", ", evaluationResults.Select(r => $"Turn{r.TurnIndex}:Result={r.Result},Score={r.Score}")));
            }

            _logger?.LogInformation(
                "[GetInterviewDetail] Statistics calculated. TotalQuestions: {TotalQuestions}, CorrectAnswers: {CorrectAnswers}, WrongAnswers: {WrongAnswers}, OverallScore: {OverallScore}",
                totalQuestions, correctAnswers, wrongAnswers, overallScore);

            // Map to nested structure (FE format): turns with answer and evaluation nested
            var turns = allTurns
                .OrderBy(t => t.TurnIndex)
                .Select(t => new TurnDetailDto
                {
                    TurnIndex = t.TurnIndex,
                    QuestionText = t.QuestionContent ?? string.Empty,
                    Topic = t.Topic,
                    Difficulty = t.Difficulty ?? string.Empty,
                    Answer = t.Answer != null ? new AnswerDto
                    {
                        UserAnswer = t.Answer.AnswerText
                    } : null,
                    Evaluation = t.Answer?.Evaluation != null ? new EvaluationDto
                    {
                        Result = t.Answer.Evaluation.Result, // Can be null if Result column missing
                        Feedback = t.Answer.Evaluation.Feedback,
                        Score = t.Answer.Evaluation.Score
                    } : null
                })
                .ToList();
            
            _logger?.LogInformation(
                "[GetInterviewDetail] Mapped turns. TurnsCount: {TurnsCount}",
                turns.Count);

            // Return both InterviewSummaryDto (for backward compatibility) and InterviewDetailDto
            // For now, we'll create InterviewSummaryDto but also support InterviewDetailDto format
            var summary = new InterviewSummaryDto
            {
                SessionId = session.Id,
                JobTitle = session.JobDescription?.Title ?? "Unknown Job",
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                WrongAnswers = wrongAnswers,
                OverallScore = overallScore,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                Evaluations = allTurns
                    .OrderBy(t => t.TurnIndex)
                    .Select(t => new TurnEvaluationDto
                    {
                        TurnIndex = t.TurnIndex,
                        Question = t.QuestionContent ?? string.Empty,
                        Topic = t.Topic,
                        Difficulty = t.Difficulty ?? string.Empty,
                        Answer = t.Answer?.AnswerText,
                        Result = t.Answer?.Evaluation?.Result,
                        Score = t.Answer?.Evaluation?.Score,
                        Feedback = t.Answer?.Evaluation?.Feedback
                    })
                    .ToList()
            };

            return Result<InterviewSummaryDto>.Success(summary);
        }

        public async Task<Result<InterviewDetailDto>> GetInterviewDetailDto(Guid sessionId, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result<InterviewDetailDto>.Fail(new Error("UNAUTHORIZED", "Invalid user"));
            }

            InterviewSession? session = null;
            bool schemaMismatch = false;
            try
            {
                session = await _interviewSessionRepository.GetSessionWithFullDepth(sessionId);
            }
            catch (SqlException sqlEx) when (
                sqlEx.Number == 207 || // Invalid column name
                sqlEx.Message.Contains("Invalid column name") ||
                sqlEx.Message.Contains("'Result'") ||
                sqlEx.Message.Contains("'CreatedAt'") ||
                sqlEx.Message.Contains("'UpdatedAt'"))
            {
                _logger?.LogWarning(sqlEx, 
                    "[GetInterviewDetailDto] Schema mismatch detected. Repository fallback should have handled this. SessionId: {SessionId}", 
                    sessionId);
                
                schemaMismatch = true;
                if (session == null)
                {
                    _logger?.LogError(sqlEx, 
                        "[GetInterviewDetailDto] Session is null after schema mismatch. SessionId: {SessionId}", 
                        sessionId);
                    return Result<InterviewDetailDto>.Fail(new Error("INTERNAL_ERROR", 
                        "Failed to load interview session. Please check database connection."));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[GetInterviewDetailDto] Unexpected error loading session. SessionId: {SessionId}", sessionId);
                return Result<InterviewDetailDto>.Fail(new Error("INTERNAL_ERROR", 
                    $"An error occurred while loading interview session: {ex.Message}"));
            }

            if (session == null)
                return Result<InterviewDetailDto>.Fail(new Error("NOT_FOUND", "Session not found"));

            if (session.UserId != userId)
                return Result<InterviewDetailDto>.Fail(new Error("FORBIDDEN", "You don't own this session"));

            var allTurns = session.Turns?.ToList() ?? new List<InterviewTurn>();
            
            if (schemaMismatch)
            {
                _logger?.LogWarning(
                    "[GetInterviewDetailDto] Schema mismatch. Returning session without evaluations. SessionId: {SessionId}, TotalTurns: {TotalTurns}",
                    sessionId, allTurns.Count);
            }

            // Map to nested structure (FE format): turns with answer and evaluation nested
            var turns = allTurns
                .OrderBy(t => t.TurnIndex)
                .Select(t => new TurnDetailDto
                {
                    TurnIndex = t.TurnIndex,
                    QuestionText = t.QuestionContent ?? string.Empty,
                    Topic = t.Topic,
                    Difficulty = t.Difficulty ?? string.Empty,
                    Answer = t.Answer != null ? new AnswerDto
                    {
                        UserAnswer = t.Answer.AnswerText
                    } : null,
                    Evaluation = t.Answer?.Evaluation != null ? new EvaluationDto
                    {
                        Result = t.Answer.Evaluation.Result, // Can be null if Result column missing
                        Feedback = t.Answer.Evaluation.Feedback,
                        Score = t.Answer.Evaluation.Score
                    } : null
                })
                .ToList();
            
            _logger?.LogInformation(
                "[GetInterviewDetailDto] Mapped turns. TurnsCount: {TurnsCount}",
                turns.Count);

            var detail = new InterviewDetailDto
            {
                SessionId = session.Id,
                JobTitle = session.JobDescription?.Title ?? "Unknown Job",
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                Turns = turns
            };

            return Result<InterviewDetailDto>.Success(detail);
        }
    }
}
