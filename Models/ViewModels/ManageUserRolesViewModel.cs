﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public DUSTUser DUSTUser { get; set; }
        public MultiSelectList Roles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
}
