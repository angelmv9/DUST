using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models
{
    public class DUSTUser : IdentityUser
    {
        // Foreign Key
        public int CompanyId { get; set; }

        [Required]
        [StringLength(25, ErrorMessage = "{0} must be at least {2} and no more than {1} characters long", MinimumLength = 3)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(25, ErrorMessage = "{0} must be at least {2} and no more than {1} characters long", MinimumLength = 1)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName { get { return $"{FirstName} {LastName}"; } }

        [NotMapped]
        [DataType(DataType.Upload)]
        [Display(Name ="Avatar Image")]
        public IFormFile AvatarFormFile { get; set; }

        public byte[] AvatarByteData { get; set; }

        [Display(Name = "File Name")]
        public string AvatarFileName { get; set; }

        [Display(Name = "File Extension")]
        public string AvatarFileExtension { get; set; }

        /* Navigation Properties */

        // Parents of DUSTUser
        public virtual Company Company { get; set; }

        // Children of DUSTUser ** many-to-many relationship with Project
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

    }
}
