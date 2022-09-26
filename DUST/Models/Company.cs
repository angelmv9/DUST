using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models
{
    public class Company
    {
        // Primary key
        public int Id { get; set; }

        [DisplayName("Company Name")]
        public string Name { get; set; }

        public string Description { get; set; }

        /* Navigation properties */

        // Children of Company
        public virtual ICollection<DUSTUser> Members { get; set; } = new HashSet<DUSTUser>();
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }
}
