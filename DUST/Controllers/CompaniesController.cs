using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DUST.Data;
using DUST.Models;
using Microsoft.AspNetCore.Authorization;
using DUST.Extensions;
using DUST.Services.Interfaces;

namespace DUST.Controllers
{
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly ICompanyInfoService _companyService;

        public CompaniesController(ICompanyInfoService companyService)
        {
            _companyService = companyService;
        }

        // GET: Company details
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return View(await _companyService.GetCompanyInfoByIdAsync(companyId));
        }
    }       
}
