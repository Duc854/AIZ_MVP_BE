using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Abstractions
{
    public interface IPaymentService
    {
        Task<object> CreatePaymentRequest(UserIdentity identity, decimal price);
        Task<bool> ProcessWebhook(string sePayContent, decimal transferAmount);
    }
}
