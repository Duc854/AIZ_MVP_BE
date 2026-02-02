using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using AIZ_MVP_Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrappers;

namespace AIZ_MVP_Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewTurnController : ControllerBase
    {
        private readonly IInterviewTurnService _interviewTurnService;
        public InterviewTurnController(IInterviewTurnService interviewTurnService)
        {
            _interviewTurnService = interviewTurnService;
        }

        [Authorize]
        [HttpPost("save-turn")]
        public async Task<IActionResult> SaveInterviewTurn([FromBody] SaveInterviewTurnDto dto)
        {
    
            if (string.IsNullOrWhiteSpace(dto.ResolvedQuestionText))
            {
                ModelState.AddModelError(
                    nameof(dto.QuestionText), 
                    "Either 'questionText' or 'questionContent' field is required");
            }

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

            var result = await _interviewTurnService.SaveInterviewTurn(dto, identity);
            if (!result.IsSuccess)
            {
                return BadRequest(new ApiResponse<object>
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

        [Authorize]
        [HttpGet("by-session-and-index")]
        public async Task<IActionResult> GetTurnIdBySessionAndIndex(
            [FromQuery] Guid sessionId,
            [FromQuery] int turnIndex)
        {
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

            var result = await _interviewTurnService.GetTurnIdBySessionAndIndex(sessionId, turnIndex, identity);
            if (!result.IsSuccess)
            {
                var statusCode = result.Error!.Code switch
                {
                    "TURN_NOT_FOUND" => StatusCodes.Status404NotFound,
                    "SESSION_NOT_FOUND" => StatusCodes.Status404NotFound,
                    "FORBIDDEN" => StatusCodes.Status403Forbidden,
                    "UNAUTHORIZED_USER" => StatusCodes.Status401Unauthorized,
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


        //API cho truong hop su dung BE de call API AI
        //[Authorize]
        //[HttpPost("generate-question")]
        //public async Task<IActionResult> GenerateQuestion([FromBody] RequestGenerateQuestionDto dto)
        //{
        //    var identity = HttpContext.GetUserIdentity();
        //    if (identity == null)
        //    {
        //        return Unauthorized(new ApiResponse<object>
        //        {
        //            Error = new ApiError
        //            {
        //                Code = "UNAUTHORIZED",
        //                Message = "User identity not found"
        //            }
        //        });
        //    }
        //    var result = await _interviewTurnService.GenerateQuestion(dto, identity);
        //    if (!result.IsSuccess)
        //    {
        //        return BadRequest(new ApiResponse<object>
        //        {
        //            Error = new ApiError
        //            {
        //                Code = result.Error!.Code,
        //                Message = result.Error!.Message
        //            }
        //        });
        //    }
        //    return Ok(new ApiResponse<QuestionResponseDto>
        //    {
        //        Data = result.Value
        //    });
        //}
    }
}
