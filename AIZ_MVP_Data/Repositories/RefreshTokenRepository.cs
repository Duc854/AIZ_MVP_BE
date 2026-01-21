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
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AIZMvpDbContext _dbContext;

        public RefreshTokenRepository(AIZMvpDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<RefreshToken?> GetValidTokenAsync(string token)
        {
            return _dbContext.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Token == token &&
                    x.ExpiredAt > DateTime.UtcNow
                );
        }

        public void Add(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Add(refreshToken);
        }
    }
}
