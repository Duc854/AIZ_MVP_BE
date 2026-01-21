using Microsoft.AspNetCore.Mvc;
using System.Runtime.ConstrainedExecution;
using Shared.Wrappers;
using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.AuthDtos;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;

namespace AIZ_MVP_Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        //Register new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]RegisterDto dto) {
            var result = await _authService.Register(dto);

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

        //Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.Login(dto);
            if (!result.IsSuccess)
                return Unauthorized(new ApiResponse<object>
                {
                    Error = new ApiError
                    {
                        Code = result.Error!.Code,
                        Message = result.Error!.Message
                    }
                });

            return Ok(new ApiResponse<LoginResponseDto>
            {
                Data = result.Value
            });
        }

        //Refresh access token
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.RefreshAccessToken(dto.RefreshToken);
            if (!result.IsSuccess)
                return Unauthorized(new ApiResponse<object>
                {
                    Error = new ApiError
                    {
                        Code = result.Error!.Code,
                        Message = result.Error!.Message
                    }
                });
            return Ok(new ApiResponse<string>
            {
                Data = result.Value
            });
        }

    }
}
