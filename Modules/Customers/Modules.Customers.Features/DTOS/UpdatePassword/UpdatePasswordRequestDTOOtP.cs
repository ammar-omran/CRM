using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Customers.Features.DTOs.UpdatePassword
{
    public class UpdatePasswordRequestDTOOtP
    {
        public string hashedEmail { get; set; }
        public string otp { get; set; }
    }
}
