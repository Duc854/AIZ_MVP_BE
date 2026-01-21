using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    public class UserUpdateProfileDto
    {
        [EmailAddress]
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}
