using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly ILicenseRepository _licenseRepo;
        private readonly IUnitOfWork _uow;

        public PaymentService(IPaymentRepository paymentRepo, ILicenseRepository licenseRepo, IUnitOfWork uow)
        {
            _paymentRepo = paymentRepo;
            _licenseRepo = licenseRepo;
            _uow = uow;
        }

        public async Task<object> CreatePaymentRequest(UserIdentity identity, decimal price)
        {
            if (!Guid.TryParse(identity.UserId, out var userId))
            {
                return Result<Guid>.Fail(
                    new Error("UNAUTHORIZED_USER", "Invalid user identity"));
            }
            string transCode = $"AIZ{DateTime.Now:ddHHmm}{new Random().Next(100, 999)}";

            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Price = price,
                Amount = price,
                TransactionCode = transCode,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(15)
            };

            _paymentRepo.Add(transaction);
            await _uow.SaveChangesAsync();
            string bank = "MBBank";
            string accNo = "123456789";
            var qrUrl = $"https://qr.sepay.vn/img?bank={bank}&acc={accNo}&template=compact&amount={price}&des={transCode}";

            return new { qrUrl, transCode };
        }

        public async Task<bool> ProcessWebhook(string sePayContent, decimal transferAmount)
        {
            var match = System.Text.RegularExpressions.Regex.Match(sePayContent, @"AIZ\s*\d+");
            if (!match.Success) return false;

            string transCode = match.Value.Replace(" ", "");

            await _uow.BeginTransactionAsync();
            try
            {
                var trans = await _paymentRepo.GetByCodeForUpdate(transCode);
                if (trans == null || transferAmount < trans.Amount || trans.Status == "Completed") return false;

                trans.Status = "Completed";

                var license = await _licenseRepo.GetLicenseByUserIdForUpdate(trans.UserId);

                if (license == null)
                {
                    license = new License
                    {
                        Id = Guid.NewGuid(),
                        UserId = trans.UserId,
                        LicenseKey = Guid.NewGuid().ToString("N").ToUpper(),
                        Plan = "Monthly",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        ExpiredAt = DateTime.UtcNow.AddMonths(1)
                    };
                    _licenseRepo.Add(license);
                }
                else
                {
                    DateTime startExtendingFrom = (license.ExpiredAt > DateTime.UtcNow)
                        ? license.ExpiredAt.Value
                        : DateTime.UtcNow;

                    license.ExpiredAt = startExtendingFrom.AddMonths(1);
                    license.IsActive = true;
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
                return true;
            }
            catch
            {
                await _uow.RollbackAsync();
                return false;
            }
        }
    }
}
