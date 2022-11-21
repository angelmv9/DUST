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
        NewDevelopment,
        /// <summary>
        /// Involves development of the specific ticket description.
        /// </summary>
        WorkTask,
        /// <summary>
        /// Involves unexpected development/maintenance on a previously designed feature/functionality.
        /// </summary>
        Defect,
        /// <summary>
        /// Involves modification development of a previously designed feature/functionality.
        /// </summary>
        ChangeRequest,
        /// <summary>
        /// Involves additional development on a previously designed feature or new functionality.
        /// </summary>
        Enhancement,
        /// <summary>
        /// Involves no software development but may involve tasks such as configuations, or hardware setup.
        /// </summary>
        GeneralTask
    }
}
