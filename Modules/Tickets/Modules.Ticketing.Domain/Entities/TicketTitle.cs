using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Domain.Entities
{
    public class TicketTitle
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int? DefaultSeverityId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
