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
        Open,
        /// <summary>
        /// Ticket is assigned and currently being worked.
        /// </summary>
        In_Progress,
        /// <summary>
        /// Ticket is assigned and it is being tested.
        /// </summary>
        Testing,
        Retest,
        Fixed,
        /// <summary>
        /// Ticket has been completed
        /// </summary>
        Closed,

        Cancelled
    }
}
