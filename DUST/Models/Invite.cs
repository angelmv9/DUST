using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models
{
    public class Invite
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Keys

        [DisplayName("Company")]
        public int CompanyId { get; set; }
        [DisplayName("Project")]
        public int ProjectId { get; set; }
        [DisplayName("Invitor")]
        public string InvitorId { get; set; }
        [DisplayName("Invitee")]
        public string InviteeId { get; set; }

        [DisplayName("Invitee Email")]
        public string InviteeEmail { get; set; }

        [DisplayName("Invitee First Name")]
        public string InviteeFirstName { get; set; }

        [DisplayName("Invitee Last Name")]
        public string InviteeLastName { get; set; }
        
        // Based on how many days have passed since the invite was sent
        public bool IsValid { get; set; }

        [DisplayName("Date Sent")]
        [DataType(DataType.Date)]
        public DateTimeOffset InviteDate { get; set; }

        [DisplayName("Join Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset JoinDate { get; set; }

        [DisplayName("Code")]
        public Guid CompanyToken { get; set; }

        /* Notification Properties */

        // Parents of Invite
        public virtual Company Company { get; set; }
        public virtual Project Project { get; set; }
        public virtual DUSTUser Invitor { get; set; }
        public virtual DUSTUser Invitee { get; set; }
    }
}
