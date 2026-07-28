using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TicketManagement.Domain.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? TitleId { get; set; }
        public int TypeId { get; set; }
        public int? SeverityId { get; set; }
        public string Title { get; set; }
        public string CustomerEmail { get; set; } 
        public string Description { get; set; }
        public TicketStatusEnum Status { get; set; } = TicketStatusEnum.Open; 
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        public int UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<TicketHistory> TicketHistories { get; set; } = [];
        public List<TicketsAttachment> ticketAttachments { get; set; }
        public List<TicketComment> ticketComments { get; set; } = [];
        public Category category { get; set; }
        public Severity Severity { get; set; }
        public TicketTitle TicketTitle { get; set; }
        public TicketType Type { get; set; }

        /// <summary>
        /// validate ticket entity
        /// </summary>
        /// <returns></returns>
        public bool validateTicket()
        {
            try
            {
                if (string.IsNullOrEmpty(Title) || Title.Length > 100)
                    return false;

                if (Description.Length > 1000)
                    return false;

                if (!string.IsNullOrEmpty(CustomerName) && CustomerName.Length > 150)
                    return false;

                if (!string.IsNullOrEmpty(CreatedByName) && CreatedByName.Length > 150)
                    return false;

                if (!string.IsNullOrEmpty(UpdatedByName) && UpdatedByName.Length > 150)
                    return false;
                //throw new ArgumentException("Subject should be less than 100 characters.");

                if (CategoryId == 0)
                    return false;

                if (TypeId == 0)
                    return false;

                if (CustomerId == 0 && CreatedBy == 0)
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
