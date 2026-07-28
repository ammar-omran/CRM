using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Customers.Features.DTOs
{
    public class UpdatePasswordRequestWithEmail
    {
        public string email { get; set; }
        public string lang { get; set; }
    }
}
