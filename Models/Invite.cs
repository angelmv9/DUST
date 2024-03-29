﻿using System;
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
        [DisplayName("Select Project")]
        public int ProjectId { get; set; }
        [DisplayName("Invitor")]
        public string InvitorId { get; set; }
        [DisplayName("Invitee")]
        public string InviteeId { get; set; }

        [DisplayName("Invitee Email")]
        [EmailAddress]
        public string InviteeEmail { get; set; }

        [DisplayName("Invitee First Name")]
        [StringLength(20, ErrorMessage = "{0} must be at least {2} and no more than {1} characters long", MinimumLength = 2)]
        public string InviteeFirstName { get; set; }

        [DisplayName("Invitee Last Name")]
        [StringLength(20, ErrorMessage = "{0} must be at least {2} and no more than {1} characters long", MinimumLength = 1)]
        public string InviteeLastName { get; set; }

        public string Description { get; set; }

        // True if invite was used to create an account
        public bool WasUsed { get; set; }

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
