using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Customers.Features.DTOs.UpdatePassword
{
    public class ResetPasswordRequestDTO
    {
        [MaxLength(200)]
        public string HashedEmail { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
