using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using AIZ_MVP_Bussiness.Services;
using AIZ_MVP_Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrappers;

namespace AIZ_MVP_Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewSesssionController : ControllerBase
    {
        private readonly IInterviewSessionService _interviewSessionService;
        public InterviewSesssionController(IInterviewSessionService interviewSessionService)
        {
            _interviewSessionService = interviewSessionService;
        }

        [Authorize]
        [HttpPost("start-interview")]
        public async Task<IActionResult> StartInterview(int jobDescriptionId)
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
            var result = await _interviewSessionService.StartInterview(identity!, jobDescriptionId);
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
            return Ok(new ApiResponse<string>()
            {
                Data = result.Value
            });
        }

        [Authorize]
        [HttpPost("end-interview")]
        public async Task<IActionResult> EndInterview(Guid interviewSessionId)
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
            var result = await _interviewSessionService.EndInterview(identity!, interviewSessionId);
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
            return Ok(new ApiResponse<object>());
        }

        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetUserHistory()
        {
            var identity = HttpContext.GetUserIdentity();
            if (identity == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Error = new ApiError { Code = "UNAUTHORIZED", Message = "User identity not found" }
                });
            }

            var result = await _interviewSessionService.GetUserInterviewHistory(identity!);
            if (!result.IsSuccess)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Error = new ApiError { Code = result.Error!.Code, Message = result.Error!.Message }
                });
            }

            return Ok(new ApiResponse<List<InterviewSessionSummaryDto>> { Data = result.Value });
        }

        [Authorize]
        [HttpGet("detail/{sessionId}")]
        public async Task<IActionResult> GetInterviewDetail(Guid sessionId)
        {
            var identity = HttpContext.GetUserIdentity();
            if (identity == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Error = new ApiError { Code = "UNAUTHORIZED", Message = "User identity not found" }
                });
            }

         
            var result = await _interviewSessionService.GetInterviewDetailDto(sessionId, identity!);
            if (!result.IsSuccess)
            {
                // Map error codes to appropriate HTTP status codes
                var statusCode = result.Error!.Code switch
                {
                    "NOT_FOUND" => 404,
                    "FORBIDDEN" => 403,
                    "UNAUTHORIZED" => 401,
                    "DB_SCHEMA_ERROR" => 500, 
                    "INTERNAL_ERROR" => 500,
                    _ => 400
                };

                return StatusCode(statusCode, new ApiResponse<object>
                {
                    Error = new ApiError { Code = result.Error.Code, Message = result.Error.Message }
                });
            }

            return Ok(new ApiResponse<InterviewDetailDto> { Data = result.Value });
        }
    }
}
