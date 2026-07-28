using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Domain.Entities
{
    public class Severity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsVisible { get; set; }
        public int Sort { get; set; }
    }
}
