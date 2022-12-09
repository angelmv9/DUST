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
    public class TicketAttachment
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key
        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        // Foreign key DUSTUser id, which is an IdentityUser
        [DisplayName("Team Member")]
        public string UserId { get; set; }

        [DisplayName("File Date")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("File Description")]
        public string FileDescription { get; set; }

        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] {".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf"})]
        public IFormFile FormFile { get; set; }

        [DisplayName("File Name")]
        public string FileName { get; set; }

        [DisplayName("File Extension")]
        public string FileExtension { get; set; }

        public byte[] ByteArrayData { get; set; }
       
        /* Navigation properties */

        // Parents of TicketAttachment

        // Allows navigation from the TicketAttachment to the ticket such attachment
        // belongs to via the TicketId foreign key.
        // The keyword virtual lets EntityFramework do lazy loading
        public virtual Ticket Ticket { get; set; }

        // Allows navigation from the TicketAttachment to its user/author
        // via the UserId foreign key.
        // The keyword virtual lets EntityFramework do lazy loading
        public virtual DUSTUser User { get; set; }

    }
}
