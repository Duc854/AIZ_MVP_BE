using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Abstractions
{
    public interface IUserRepository
    {
        void Add(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserToUpdateAsync(Guid userId);
        Task<User?> GetUserById(Guid userId);
    }
}
