using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Context;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AIZMvpDbContext _context;
        private IDbContextTransaction _transaction;
        public UnitOfWork(AIZMvpDbContext context) => _context = context;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
                => await _context.SaveChangesAsync(cancellationToken);

        public async Task BeginTransactionAsync()
            => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null) await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose() => _context.Dispose();
    }
}
