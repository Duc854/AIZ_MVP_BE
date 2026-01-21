using AIZ_MVP_Bussiness.Dtos.AuthDtos;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Abstractions
{
    public interface IAuthService
    {
        Task<Result> Register(RegisterDto dto);
        Task<Result<LoginResponseDto>> Login(LoginDto dto);
        Task<Result<string>> RefreshAccessToken(string refreshToken);
    }
}
