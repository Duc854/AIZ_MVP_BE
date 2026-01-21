using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Context;
using AIZ_MVP_Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AIZMvpDbContext _dbContext;
        public UserRepository(AIZMvpDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void Add(User user)
        {
            _dbContext.Users.Add(user);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            return await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserToUpdateAsync(Guid userId)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
