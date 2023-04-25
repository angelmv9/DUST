using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models.Enums
{
    public enum TicketTypeEnum
    {
        /// <summary>
        /// Involves development of a new, uncoded solution.
        /// </summary>
        New_Feature,
        /// <summary>
        /// Involves development of the specific ticket description.
        /// </summary>
        Task,
        /// <summary>
        /// Involves unexpected development/maintenance on a previously designed feature/functionality.
        /// </summary>
        Bug,
        /// <summary>
        /// Involves modification development of a previously designed feature/functionality.
        /// </summary>
        Change_Request,
        /// <summary>
        /// Involves additional development on a previously designed feature or new functionality.
        /// </summary>
        Improvement,
        /// <summary>
        /// Involves no software development but may involve tasks such as configuations, or hardware setup.
        /// </summary>
        Test
    }
}
