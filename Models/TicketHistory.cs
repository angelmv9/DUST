using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models
{
    public class TicketHistory
    {
        // primary key for Entity Framework
        public int Id { get; set; }

        // Foreign key
        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        // Foreign key DUSTUser id, which is an IdentityUser
        [DisplayName("Team Member")]
        public string UserId { get; set; }

        [DisplayName("Updated Item")]
        public string Property { get; set; }

        [DisplayName("Previous")]
        public string OldValue { get; set; }

        [DisplayName("Current")]
        public string NewValue { get; set; }

        [DisplayName("Date Modified")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Description of Change")]
        public string Description { get; set; }

        /* Navigation properties */

        //Parents of TicketHistory

        // Allows navigation from the TicketHistory to the Ticket such history belongs to
        // via the foreign key (a.k.a. a pointer) TicketId
        // the keyword virtual lets EntityFramework do lazy loading 
        public virtual Ticket ticket { get; set; }
        // Allows navigation from the TicketHistory to its user/author via the
        // foreign key (a.k.a. a pointer) UserId
        // the keyword virtual lets EntityFramework do lazy loading 
        public virtual DUSTUser User { get; set; }
    }
}
