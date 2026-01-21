using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Data.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = null!;
        public DateTime ExpiredAt { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
