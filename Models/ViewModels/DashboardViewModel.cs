using System.Collections.Generic;

namespace DUST.Models.ViewModels
{
    public class DashboardViewModel
    {
        public Company Company { get; set; }
        public List<Project> Projects { get; set; }
        public List<Ticket> Tickets { get; set; }
        public List<DUSTUser> Members { get; set; }
    }
}
