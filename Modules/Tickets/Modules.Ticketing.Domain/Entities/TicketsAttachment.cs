using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Domain.Entities
{
    public class TicketsAttachment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string FileType {  get; set; }
        public string FileName {  get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }

        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public Ticket Ticket { get; set; }
    }
}
