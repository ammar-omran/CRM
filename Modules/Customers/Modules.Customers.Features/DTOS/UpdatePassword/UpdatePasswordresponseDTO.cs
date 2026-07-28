using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Customers.Features.DTOs.UpdatePassword
{
    public class UpdatePasswordresponseDTO(bool statusp, string messagep, OtpDTO otpp)
    {
        public bool status { get; set; } = statusp;
        public string message { get; set; } = messagep;
        public OtpDTO otp { get; set; } = otpp;
    }
}
