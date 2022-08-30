using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models
{
    public class Notification
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Keys

        public int TicketId { get; set; }

        [Required]
        [DisplayName("Recipient")]
        public string RecipientId { get; set; }
        [Required]
        [DisplayName("Sender")]
        public string SenderId { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }

        [DataType(DataType.Date)]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Has been viewed")]
        public bool Viewed { get; set; }

        /* Navigation Properties */

        // Parents of Notification
        public virtual Ticket Ticket { get; set; }
        public virtual DUSTUser Recipient { get; set; }
        public virtual DUSTUser Sender { get; set; }
    }
}
