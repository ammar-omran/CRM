using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Domain.Entities
{
    public class TicketComment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int? CreatedBy { get; set; }                                                                               
        public string? CreatedByName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Ticket Ticket { get; set; } = default!;
        public DateTime CreatedDate { get; set; } = DateTime.Now;


        /// <summary>
        /// Validate TicketComment entity
        /// </summary>
        public bool ValidateTicketComment()
        {
            try
            {
                if (string.IsNullOrEmpty(CreatedByName) || CreatedByName.Length > 150)
                    return false;

                if (string.IsNullOrEmpty(Description) || Description.Length > 1000)
                    return false;

                if (TicketId <= 0)
                    return false;

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
