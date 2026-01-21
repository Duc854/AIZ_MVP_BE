using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Entities
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public string TransactionCode { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public string? DiscountCode { get; set; }
        public decimal Discount { get; set; }
        public DateTime ExpiredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
