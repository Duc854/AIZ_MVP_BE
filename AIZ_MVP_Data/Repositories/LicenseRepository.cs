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
    public class LicenseRepository : ILicenseRepository
    {
        private readonly AIZMvpDbContext _dbContext;
        public LicenseRepository(AIZMvpDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<License?> GetLicenseByUserId(Guid userId)
        {
            return await _dbContext.Licenses.AsNoTracking().FirstOrDefaultAsync(l => l.UserId == userId);
        }

        public async Task<License?> GetLicenseByUserIdForUpdate(Guid userId)
        {
            return await _dbContext.Licenses.FirstOrDefaultAsync(l => l.UserId == userId);
        }
        public async Task<License?> GetValidLicenseAsync(Guid userId)
        {
            var now = DateTime.UtcNow;

            return await _dbContext.Licenses
                .FirstOrDefaultAsync(l =>
                    l.UserId == userId &&
                    l.IsActive &&
                    (
                        l.ExpiredAt == null ||
                        l.ExpiredAt > now
                    )
                );
        }

        public void Add(License license)
        {
            _dbContext.Licenses.Add(license);
        }

        public void Remove(License license)
        {
            _dbContext.Licenses.Remove(license);
        }

        public async Task<License?> GetFreeTrialLicenseAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Licenses
                .FirstOrDefaultAsync(l =>
                    l.UserId == userId &&
                    l.IsActive &&
                    l.LicenseKey.StartsWith("FREE")
                    &&
                    (
                        l.ExpiredAt == null ||
                        l.ExpiredAt > now
                    )
                    );
        }

    }
}
