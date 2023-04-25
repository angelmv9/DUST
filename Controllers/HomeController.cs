using DUST.Extensions;
using DUST.Models;
using DUST.Models.ChartModels;
using DUST.Models.Enums;
using DUST.Models.ViewModels;
using DUST.Services;
using DUST.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICompanyInfoService _companyService;
        private readonly IProjectService _projectService;
        private readonly ILogger<HomeController> _logger;
		private readonly SignInManager<DUSTUser> _signInManager;

		public HomeController(ILogger<HomeController> logger,
			ICompanyInfoService companyService,
			IProjectService projectService,
			SignInManager<DUSTUser> signInManager)
		{
			_logger = logger;
			_companyService = companyService;
			_projectService = projectService;
			_signInManager = signInManager;
		}

		public IActionResult Index()
        {
			if(_signInManager.IsSignedIn(User))
			{
				return RedirectToAction("Dashboard", "Home");
			}
            return View();
        }

		[Authorize]
        public async Task<IActionResult> Dashboard()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            DashboardViewModel model = new();
            model.Company = await _companyService.GetCompanyInfoByIdAsync(companyId);
            model.Projects = (await _companyService.GetAllProjectsAsync(companyId)).Where(p => p.Archived == false).ToList();
            model.Tickets = model.Projects.SelectMany(p => p.Tickets).Where(t => t.Archived == false).ToList();
            model.Members = model.Company.Members.ToList();

            return View(model);
        }

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
