using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models
{
    public class Ticket
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key
        [DisplayName("Project ID")]
        public int ProjectId { get; set; }

        // Foreign key
        [DisplayName("Ticket Type")]
        public int TicketTypeId { get; set; }

        // Foreign key
        [DisplayName("Ticket Priority")]
        public int TicketPriorityId { get; set; }

        // Foreign key
        [DisplayName("Ticket Status")]
        public int TicketStatusId { get; set; }

        // Foreign key of type DUSTUser
        [DisplayName("Ticket Owner")]
        public string OwnerUserId { get; set; }

        // Foreign key of type DUSTUser
        [DisplayName("Ticket Developer")]
        public string DeveloperUserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTimeOffset Created { get; set; }

        // Can be null
        [DataType(DataType.Date)]
        public DateTimeOffset? Updated { get; set; }

        public bool Archived { get; set; }

        [DisplayName("Archived by Project")]
        public bool ArchivedByProject { get; set; }

        /* Navigation Properties*/

        // ** Parents of Ticket **

        // Allows navigation from the Ticket to the Project the ticket is part of via
        // the ProjectId foreign key
        public virtual Project Project { get; set; }

        // Allows navigation from the Ticket to the creator of the ticket via
        // the OwnerUserId foreign key
        [DisplayName("Owner")]
        public virtual DUSTUser OwnerUser { get; set; }

        // Allows navigation from the Ticket to the developer assigned to the ticket via
        // the DeveloperUserId foreign key
        [DisplayName("Developer")]
        public virtual DUSTUser DeveloperUser { get; set; }

        // ** Children of Ticket. ** 1-to-many relationship
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();

        // ** Lookup tables **
        [DisplayName("Type")]
        public virtual TicketType TicketType { get; set; }
        [DisplayName("Priority")]
        public virtual TicketPriority TicketPriority { get; set; }
        [DisplayName("Status")]
        public virtual TicketStatus TicketStatus { get; set; }
    }
}
