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
    public class InterviewAnswerController : ControllerBase
    {
        private readonly IInterviewAnswerService _interviewAnswerService;
        public InterviewAnswerController(IInterviewAnswerService interviewAnswerService)
        {
            _interviewAnswerService = interviewAnswerService;
        }

        [Authorize]
        [HttpPost("save-answer")]
        public async Task<IActionResult> SaveInterviewAnswer([FromBody] SaveInterviewAnswerDto dto)
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

            var result = await _interviewAnswerService.SaveAnswer(dto, identity);
            if (!result.IsSuccess)
            {
                // Map error codes to appropriate HTTP status codes
                var statusCode = result.Error!.Code switch
                {
                    "TURN_NOT_FOUND" => StatusCodes.Status404NotFound,
                    "FORBIDDEN" => StatusCodes.Status403Forbidden,
                    "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
                    "DB_ERROR" => StatusCodes.Status500InternalServerError,
                    _ => StatusCodes.Status400BadRequest
                };

                return StatusCode(statusCode, new ApiResponse<object>
                {
                    Error = new ApiError
                    {
                        Code = result.Error!.Code,
                        Message = result.Error!.Message
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
