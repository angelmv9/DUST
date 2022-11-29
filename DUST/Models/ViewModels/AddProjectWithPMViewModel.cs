using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models.ViewModels
{
    public class AddProjectWithPMViewModel
    {
        public Project Project { get; set; }
        public SelectList PMList { get; set; }
        public string PMId { get; set; }
        public SelectList PriorityList { get; set; }       
    }
}
