using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Customers.Features.DTOs.UpdatePassword
{
    public class OtpDTO
    {
        public string otpCode { get; set; } 
        public DateTime createdDate { get; set; }
    }
}
