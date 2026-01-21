using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Entities
{
    public class License
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string LicenseKey { get; set; } = null!;
        public DateTime? ExpiredAt { get; set; }
        public string Plan { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsValid(DateTime now)
        {
            if (!IsActive) return false;
            if (ExpiredAt.HasValue && ExpiredAt <= now) return false;
            return true;
        }
    }
}
