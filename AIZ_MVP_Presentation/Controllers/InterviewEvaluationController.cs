using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using AIZ_MVP_Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrappers;
using System.Linq;

namespace AIZ_MVP_Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewEvaluationController : ControllerBase
    {
        private readonly IInterviewEvaluationService _interviewEvaluationService;
        public InterviewEvaluationController(IInterviewEvaluationService interviewEvaluationService)
        {
            _interviewEvaluationService = interviewEvaluationService;
        }

        [Authorize]
        [HttpPost("save-evaluation")]
        public async Task<IActionResult> SaveInterviewEvaluation([FromBody] SaveEvaluationDto dto)
        {
            // Validate ModelState (data annotations)
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => new
                    {
                        Field = x.Key,
                        Message = e.ErrorMessage ?? "Invalid value"
                    }))
                    .ToList();

                var errorDetails = string.Join("; ", validationErrors.Select(e => $"{e.Field}: {e.Message}"));

                return BadRequest(new ApiResponse<object>
                {
                    Error = new ApiError
                    {
                        Code = "VALIDATION_ERROR",
                        Message = $"Request validation failed. Errors: {errorDetails}"
                    }
                });
            }

            // Custom validation: Check that at least one identifier is provided
            if (!dto.IsValid())
            {
                return BadRequest(new ApiResponse<object>
                {
                    Error = new ApiError
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Either InterviewAnswerId must be provided, or both InterviewSessionId and TurnIndex must be provided"
                    }
                });
            }

            var identity = HttpContext.GetUserIdentity();
            if (identity == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Error = new ApiError
                    {
                        Code = "UNAUTHORIZED",
                        Message = "User identity not found"
                    }
                });
            }
            var result = await _interviewEvaluationService.SaveEvaluation(dto, identity);
            if (!result.IsSuccess)
            {
                // Map error codes to appropriate HTTP status codes
                var statusCode = result.Error!.Code switch
                {
                    "VALIDATION_ERROR" => 400,
                    "ANSWER_NOT_FOUND" => 404,
                    "FORBIDDEN" => 403,
                    "UNAUTHORIZED" => 401,
                    "EVALUATION_ALREADY_EXISTS" => 409, // Conflict
                    "DB_ERROR" => 500,
                    "INTERNAL_ERROR" => 500,
                    _ => 400
                };

                return StatusCode(statusCode, new ApiResponse<object>
                {
                    Error = new ApiError
                    {
                        Code = result.Error.Code,
                        Message = result.Error.Message
                    }
                });
            }
            return Ok(new ApiResponse<Guid>
            {
                Data = result.Value
            });
        }
    }
}
