using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models
{
    public class TicketType
    {
        // primary key for Entity Framework
        public int Id { get; set; }

        [DisplayName("Ticket Name")]
        public string Name { get; set; }
    }
}
