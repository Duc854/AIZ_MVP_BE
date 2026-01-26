using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Entities;
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
    public class InterviewEvaluationService : IInterviewEvaluationService
    {
        private readonly IInterviewEvaluationRepository _interviewEvaluationRepository;
        private readonly IInterviewAnswerRepository _interviewAnswerRepository;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<InterviewEvaluationService> _logger;

        public InterviewEvaluationService(
            IInterviewEvaluationRepository interviewEvaluationRepository, 
            IInterviewAnswerRepository interviewAnswerRepository, 
            IUnitOfWork uow,
            ILogger<InterviewEvaluationService> logger)
        {
            _interviewEvaluationRepository = interviewEvaluationRepository;
            _interviewAnswerRepository = interviewAnswerRepository;
            _uow = uow;
            _logger = logger;
        }
        public async Task<Result<Guid>> SaveEvaluation(SaveEvaluationDto dto, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
                return Result<Guid>.Fail(new Error("UNAUTHORIZED", "Invalid user identity"));

            // Log incoming DTO (without sensitive info)
            var lookupMethod = dto.GetLookupMethod();
            _logger.LogInformation(
                "[SAVE_EVALUATION] Incoming request. LookupMethod: {LookupMethod}, InterviewAnswerId: {AnswerId}, InterviewSessionId: {SessionId}, TurnIndex: {TurnIndex}, Result: {Result}",
                lookupMethod,
                dto.InterviewAnswerId?.ToString() ?? "null",
                dto.InterviewSessionId?.ToString() ?? "null",
                dto.TurnIndex?.ToString() ?? "null",
                dto.Result);

            // Validate that at least one identifier is provided
            if (!dto.IsValid())
            {
                _logger.LogWarning("[SAVE_EVALUATION] Validation failed: No valid identifier provided");
                return Result<Guid>.Fail(new Error("VALIDATION_ERROR", 
                    "Either InterviewAnswerId must be provided (and not empty), or both InterviewSessionId and TurnIndex must be provided"));
            }

            InterviewAnswer? answer = null;
            string lookupPath = "";

            // Priority 1: If InterviewAnswerId is provided and not empty GUID, load by ID
            if (dto.InterviewAnswerId.HasValue && dto.InterviewAnswerId.Value != Guid.Empty)
            {
                lookupPath = "Option 1: InterviewAnswerId";
                _logger.LogInformation("[SAVE_EVALUATION] Using {LookupPath}. AnswerId: {AnswerId}", lookupPath, dto.InterviewAnswerId.Value);
                
                // Query by InterviewAnswerId (not InterviewTurnId) - ensure correct GUID field
                answer = await _interviewAnswerRepository.GetAnswerById(dto.InterviewAnswerId.Value);
                
                if (answer == null)
                {
                    _logger.LogWarning("[SAVE_EVALUATION] Answer not found by InterviewAnswerId: {AnswerId}", dto.InterviewAnswerId.Value);
                }
                else
                {
                    _logger.LogInformation("[SAVE_EVALUATION] Answer found. AnswerId: {AnswerId}, TurnId: {TurnId}, SessionId: {SessionId}", 
                        answer.Id, answer.InterviewTurnId, answer.InterviewTurn.InterviewSessionId);
                }
            }
            // Priority 2: Fallback to loading by InterviewSessionId + TurnIndex
            else if (dto.InterviewSessionId.HasValue && dto.TurnIndex.HasValue)
            {
                lookupPath = "Option 2: InterviewSessionId + TurnIndex";
                _logger.LogInformation("[SAVE_EVALUATION] Using {LookupPath}. SessionId: {SessionId}, TurnIndex: {TurnIndex}", 
                    lookupPath, dto.InterviewSessionId.Value, dto.TurnIndex.Value);
                
                answer = await _interviewAnswerRepository.GetAnswerBySessionAndTurn(
                    dto.InterviewSessionId.Value, 
                    dto.TurnIndex.Value);
                
                if (answer == null)
                {
                    _logger.LogWarning("[SAVE_EVALUATION] Answer not found by SessionId: {SessionId}, TurnIndex: {TurnIndex}", 
                        dto.InterviewSessionId.Value, dto.TurnIndex.Value);
                }
                else
                {
                    _logger.LogInformation("[SAVE_EVALUATION] Answer found. AnswerId: {AnswerId}", answer.Id);
                }
            }

            if (answer == null)
            {
                _logger.LogError("[SAVE_EVALUATION] ANSWER_NOT_FOUND. LookupPath: {LookupPath}", lookupPath);
                return Result<Guid>.Fail(new Error("ANSWER_NOT_FOUND", 
                    $"Cannot find interview answer using {lookupPath}. Please verify the InterviewAnswerId or InterviewSessionId + TurnIndex are correct."));
            }

            // Validate ownership
            if (answer.InterviewTurn.InterviewSession.UserId != userId)
            {
                _logger.LogWarning("[SAVE_EVALUATION] FORBIDDEN. AnswerId: {AnswerId}, AnswerOwner: {AnswerOwner}, RequestUserId: {RequestUserId}", 
                    answer.Id, answer.InterviewTurn.InterviewSession.UserId, userId);
                return Result<Guid>.Fail(new Error("FORBIDDEN", "You do not own this interview session"));
            }

            // Check if evaluation already exists for this answer (upsert logic)
            InterviewEvaluation? existingEvaluation = null;
            try
            {
                existingEvaluation = await _interviewEvaluationRepository.GetEvaluationByAnswerId(answer.Id);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Result") || ex.Message.Contains("schema mismatch"))
            {
                _logger.LogError(ex, "[SAVE_EVALUATION] Database schema error. AnswerId: {AnswerId}. Migration may not be applied.", answer.Id);
                return Result<Guid>.Fail(new Error("DB_SCHEMA_ERROR", 
                    "Database schema is out of sync. Please apply migrations. Error: Missing 'Result' column in InterviewEvaluations table."));
            }
            
            InterviewEvaluation evaluation;
            bool isUpdate = existingEvaluation != null;
            
            _logger.LogInformation("[SAVE_EVALUATION] Evaluation lookup. AnswerId: {AnswerId}, ExistingEvaluation: {Exists}", 
                answer.Id, existingEvaluation != null ? "Yes" : "No");

            if (existingEvaluation != null)
            {
                _logger.LogInformation("[SAVE_EVALUATION] Updating existing evaluation. EvaluationId: {EvaluationId}, AnswerId: {AnswerId}", 
                    existingEvaluation.Id, answer.Id);
                
                // Update existing evaluation
                evaluation = existingEvaluation;
                evaluation.Result = dto.Result;
                evaluation.Score = dto.Score;
                evaluation.Feedback = dto.Feedback;
                // Note: PhoneticFeedback not in entity yet, would need to add if required
            }
            else
            {
                _logger.LogInformation("[SAVE_EVALUATION] Creating new evaluation. AnswerId: {AnswerId}", answer.Id);
                
                // Create new evaluation
                evaluation = new InterviewEvaluation
                {
                    Id = Guid.NewGuid(),
                    InterviewAnswerId = answer.Id,
                    Result = dto.Result, // PASS or FAIL
                    Score = dto.Score, // Optional
                    Feedback = dto.Feedback,
                    CreatedAt = DateTime.UtcNow
                };
                _interviewEvaluationRepository.Add(evaluation);
            }

            try
            {
                await _uow.SaveChangesAsync();
                _logger.LogInformation(
                    "[SAVE_EVALUATION] Success. EvaluationId: {EvaluationId}, IsUpdate: {IsUpdate}, AnswerId: {AnswerId}, Result: {Result}, Score: {Score}",
                    evaluation.Id, isUpdate, answer.Id, evaluation.Result, evaluation.Score?.ToString() ?? "NULL");
                return Result<Guid>.Success(evaluation.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "[SAVE_EVALUATION] DB_ERROR. AnswerId: {AnswerId}", answer.Id);
                
                // Check if it's a unique constraint violation (evaluation already exists)
                if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE") || 
                    ex.InnerException != null && ex.InnerException.Message.Contains("duplicate"))
                {
                    return Result<Guid>.Fail(new Error("EVALUATION_ALREADY_EXISTS", 
                        "An evaluation already exists for this answer. Please use update instead."));
                }
                
                return Result<Guid>.Fail(new Error("DB_ERROR", $"Cannot save interview evaluation: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SAVE_EVALUATION] Unexpected error. AnswerId: {AnswerId}", answer.Id);
                return Result<Guid>.Fail(new Error("INTERNAL_ERROR", $"Unexpected error: {ex.Message}"));
            }
        }
    }
}
