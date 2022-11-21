using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models.Enums
{
    public enum TicketStatusEnum
    {
        /// <summary>
        /// Newly created ticket having never been assigned.
        /// </summary>
        New,
        /// <summary>
        /// Ticket is assigned and currently being worked.
        /// </summary>
        Development,
        /// <summary>
        /// Ticket is assigned and it is being tested.
        /// </summary>
        Testing,
        /// <summary>
        /// Ticket has been completed
        /// </summary>
        Closed
    }
}
