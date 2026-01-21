using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _uow;
        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork uow)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _uow = uow; 
        }

        public async Task<Result> ChangePassword(UserChangePasswordDto dto, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result.Fail(
                    new Error("UNAUTHORIZED_USER", "Invalid user identity"));
            }

            var user = await _userRepository.GetUserToUpdateAsync(userId);
            if (user == null)
            {
                return Result.Fail(
                    new Error("USER_NOT_FOUND", "Cannot find current user"));
            }

            if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return Result.Fail(
                    new Error("INVALID_PASSWORD", "Current password is incorrect"));
            }
            if (_passwordHasher.Verify(dto.NewPassword, user.PasswordHash))
            {
                return Result.Fail(
                    new Error("PASSWORD_REUSED", "New password must be different from old password"));
            }
            user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            try
            {
                await _uow.SaveChangesAsync();
                return Result.Success();
            }
            catch (DbUpdateException)
            {
                return Result.Fail(
                    new Error("DB_ERROR", "Cannot change password")
                );
            }
        }

        public async Task<Result> UpdateProfile(UserUpdateProfileDto dto, UserIdentity identity)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result.Fail(
                    new Error("UNAUTHORIZED_USER", "Invalid user identity"));
            }

            if (string.IsNullOrWhiteSpace(dto.FullName) && dto.Email == null)
            {
                return Result.Fail(
                    new Error("NO_DATA", "No data to update")
                );
            }

            var user = await _userRepository.GetUserToUpdateAsync(userId);
            if (user == null) {
                return Result.Fail(
                    new Error("USER_NOT_FOUND", "Cannot find current user"));
            }
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;
            if (dto.Email != null)
            {
                var exists = await _userRepository.GetByEmailAsync(dto.Email);
                if (exists != null && exists.Id != user.Id)
                {
                    return Result.Fail(
                        new Error("EMAIL_EXISTS", "Email already in use")
                    );
                }
                user.Email = dto.Email;
            }
            try
            {
                await _uow.SaveChangesAsync();
                return Result.Success();
            }
            catch (DbUpdateException)
            {
                return Result.Fail(
                    new Error("DB_ERROR", "Cannot update user profile")
                );
            }
        }

    }
}
