using AIZ_MVP_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Extensions
{
    public class LicenseFactory
    {
        public static License CreateFreeLicense(Guid userId)
        {
            return new License
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LicenseKey = $"FREE-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Plan = "free",
                IsActive = true
            };
        }
    }
}
