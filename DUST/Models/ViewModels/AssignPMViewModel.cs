using Microsoft.AspNetCore.Mvc.Rendering;

namespace DUST.Models.ViewModels
{
    public class AssignPMViewModel
    {
        public Project Project { get; set; }
        public SelectList ProjectManagers { get; set; }
        public string PMId { get; set; }
    }
}
