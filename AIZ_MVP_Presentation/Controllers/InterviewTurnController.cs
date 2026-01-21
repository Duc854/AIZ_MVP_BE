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
            return Ok(new ApiResponse<object>());
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
