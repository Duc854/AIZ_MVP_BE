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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AIZMvpDbContext _dbContext;
        public PaymentRepository(AIZMvpDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaymentTransaction?> GetByCodeForUpdate(string code)
        {
            return await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.TransactionCode == code && t.Status == "Pending");
        }

        public void Add(PaymentTransaction transaction)
        {
            _dbContext.PaymentTransactions.Add(transaction);
        }
    }
}
