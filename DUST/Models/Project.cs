using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models
{
    public class Project
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key
        public int? CompanyId { get; set; }

        // Foreign Key
        [DisplayName("Project Priority")]
        public int? ProjectPriorityId { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Project Name")]
        public string Name { get; set; }

        public string Description { get; set; }

        [DisplayName("Start Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset StartDate { get; set; }

        [DisplayName("End Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset EndDate { get; set; }

        public bool Archived { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile ImageFormFile { get; set; }

        [DisplayName("File Name")]
        public string ImageFileName { get; set; }

        // The byte array data
        public byte[] ImageFileData { get; set; }

        [DisplayName("File Extension")]
        public string FileExtention { get; set; }
                
        /* Navigation Properties */

        // Parents of Project
        public virtual Company Company { get; set; }

        // Children of Project

        // ** 1-to-many relationship with Ticket
        public virtual ICollection<Ticket> Tickets { get; set; }  = new HashSet<Ticket>();

        // ** many-to-many relationship with DUSTUser
        public virtual ICollection<DUSTUser> Members { get; set; } = new HashSet<DUSTUser>();

        // Lookup tables
        public virtual ProjectPriority ProjectPriority { get; set; }
    }
}
