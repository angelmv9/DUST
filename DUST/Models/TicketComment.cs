using System;
using System.ComponentModel;

namespace DUST.Models
{
    public class TicketComment
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key for DUSTUser 
        [DisplayName("Team Member")]
        public string UserId { get; set; }

        // Foreign key
        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Member Comment")]
        public string Comment { get; set; }

        [DisplayName("Comment Date")]
        public DateTimeOffset Created { get; set; }

        /* Navigation Properties */

        // Parents of TicketComment

        // Allows navigation from TicketComment to the user the comment belongs to
        // via the UserId foreign key.
        // The virtual keyboard allows Entity Framework to use lazy loading
        public virtual DUSTUser User { get; set; }

        // Allows navigation from TicketComment to the ticket the comment belongs to
        // via the TicketId foreign key.
        // The virtual keyboard allows Entity Framework to use lazy loading
        public virtual Ticket Ticket { get; set; }
    }
}
