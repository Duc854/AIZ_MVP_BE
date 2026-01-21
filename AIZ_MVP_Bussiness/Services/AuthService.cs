using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Data.Abstractions;
using Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIZ_MVP_Data.Entities;
using Microsoft.EntityFrameworkCore;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using AIZ_MVP_Bussiness.Dtos.AuthDtos;
using AIZ_MVP_Data.Repositories;

namespace AIZ_MVP_Bussiness.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenProvider _tokenProvider;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _uow;
        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenProvider tokenProvider,
            IRefreshTokenRepository refreshTokenRepository, IUnitOfWork uow)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenProvider = tokenProvider;
            _refreshTokenRepository = refreshTokenRepository;   
            _uow = uow;
        }

        //Register new user validate email form, unique email and confirm passowrd
        public async Task<Result> Register(RegisterDto dto)
        {
            var existed = await _userRepository.GetByEmailAsync(dto.Email);
            if (existed != null)
            {
                return Result.Fail(
                    new Error("EMAIL_EXISTS", "Email already exists")
                );
            }
             _userRepository.Add(
                new User
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = _passwordHasher.HashPassword(dto.Password)
                });

            try
            {
                await _uow.SaveChangesAsync();
                return Result.Success();
            }
            catch (DbUpdateException)
            {
                return Result.Fail(
                    new Error("DB_ERROR", "Cannot create user")
                );
            }
        }

        //User login
        public async Task<Result<LoginResponseDto>> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                return Result<LoginResponseDto>.Fail(
                    new Error("INVALID_CREDENTIALS", "Email or password is incorrect"));
            }

            var validPassword = _passwordHasher.Verify(dto.Password, user.PasswordHash);
            if (!validPassword)
            {
                return Result<LoginResponseDto>.Fail(
                    new Error("INVALID_CREDENTIALS", "Email or password is incorrect"));
            }

            var accessToken = _tokenProvider.GenerateAccessToken(
                user.Id.ToString(),
                user.Email,
                user.Role
            );

            var refreshToken = _tokenProvider.GenerateRefreshToken();

            _refreshTokenRepository.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiredAt = DateTime.UtcNow.AddDays(30)
            });
            try
            {
                await _uow.SaveChangesAsync();
                return Result<LoginResponseDto>.Success(new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                });
            }
            catch (DbUpdateException)
            {
                return Result<LoginResponseDto>.Fail(
                    new Error("DB_ERROR", "Cannot create user")
                );
            }
        }

        public async Task<Result<string>> RefreshAccessToken(string refreshToken)
        {
            var storedToken = await _refreshTokenRepository.GetValidTokenAsync(refreshToken);

            if (storedToken == null)
            {
                return Result<string>.Fail(
                    new Error("INVALID_REFRESH_TOKEN", "Refresh token is invalid or expired")
                );
            }

            var user = storedToken.User;

            var newAccessToken = _tokenProvider.GenerateAccessToken(
                user.Id.ToString(),
                user.Email,
                user.Role
            );

            return Result<string>.Success(newAccessToken);
        }
    }
}
