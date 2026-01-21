using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Abstractions
{
    public interface ITokenProvider
    {
        string GenerateAccessToken(string userId, string username, string role);
        string GenerateRefreshToken();
    }
}
