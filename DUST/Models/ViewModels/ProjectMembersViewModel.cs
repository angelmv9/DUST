using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DUST.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project Project { get; set; }
        public MultiSelectList Users { get; set; }
        public List<string> SelectedUsersIds { get; set; }
    }
}
