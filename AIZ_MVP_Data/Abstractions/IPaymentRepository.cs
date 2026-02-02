using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Abstractions
{
    public interface IPaymentRepository
    {
        Task<PaymentTransaction?> GetByCodeForUpdate(string code);
        void Add(PaymentTransaction transaction);
    }
}
