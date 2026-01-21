using AIZ_MVP_Bussiness.Dtos.RequestDtos;
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
    public class InterviewEvaluationController : ControllerBase
    {
        private readonly InterviewEvaluationService _interviewEvaluationService;
        public InterviewEvaluationController(InterviewEvaluationService interviewEvaluationService)
        {
            _interviewEvaluationService = interviewEvaluationService;
        }

        [Authorize]
        [HttpPost("save-evaluation")]
        public async Task<IActionResult> SaveInterviewEvaluation([FromBody] SaveEvaluationDto dto)
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
            var result = await _interviewEvaluationService.SaveEvaluation(dto, identity);
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
    }
}
