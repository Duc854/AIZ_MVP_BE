using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIZ_MVP_Bussiness.Dtos.RequestDtos
{
    public class UserChangePasswordDto
    {
        [Required]
        [MinLength(8, ErrorMessage = "Current password is invalid")]
        public string CurrentPassword { get; set; } = null!;
        [Required]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters")]
        public string NewPassword { get; set; } = null!;
        [Required]
        [Compare("NewPassword", ErrorMessage = "Confirm password does not match")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
