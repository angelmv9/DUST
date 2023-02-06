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


		[HttpPost]
		public async Task<JsonResult> GglProjectTickets()
		{
			int companyId = User.Identity.GetCompanyId().Value;

			List<Project> projects = await _projectService.GetAllActiveProjectsByCompanyAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "ProjectName", "TicketCount" });

			foreach (Project prj in projects)
			{
				chartData.Add(new object[] { prj.Name, prj.Tickets.Count() });
			}

			return Json(chartData);
		}

		[HttpPost]
		public async Task<JsonResult> GglProjectPriority()
		{
			int companyId = User.Identity.GetCompanyId().Value;

			List<Project> projects = await _projectService.GetAllActiveProjectsByCompanyAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "Priority", "Count" });


			foreach (string priority in Enum.GetNames(typeof(ProjectPriorityEnum)))
			{
				int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
				chartData.Add(new object[] { priority, priorityCount });
			}

			return Json(chartData);
		}

		[HttpPost]
		public async Task<JsonResult> AmCharts()
		{
			AmChartData amChartData = new();
			List<AmItem> amItems = new();
			int companyId = User.Identity.GetCompanyId().Value;

			List<Project> projects = (await _companyService.GetAllProjectsAsync(companyId)).Where(p => p.Archived == false).ToList();

			foreach (Project project in projects)
			{
				AmItem item = new();

				item.Project = project.Name;
				item.Tickets = project.Tickets.Count;
				item.Developers = (await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(RolesEnum.Developer))).Count();

				amItems.Add(item);
			}

			amChartData.Data = amItems.ToArray();

			return Json(amChartData.Data);
		}

		[HttpPost]
		public async Task<JsonResult> PlotlyBarChart()
		{
			PlotlyBarData plotlyData = new();
			List<PlotlyBar> barData = new();

			int companyId = User.Identity.GetCompanyId().Value;

			List<Project> projects = await _projectService.GetAllActiveProjectsByCompanyAsync(companyId);

			//Bar One
			PlotlyBar barOne = new()
			{
				X = projects.Select(p => p.Name).ToArray(),
				Y = projects.SelectMany(p => p.Tickets).GroupBy(t => t.ProjectId).Select(g => g.Count()).ToArray(),
				Name = "Tickets",
				Type = "bar"
			};

			//Bar Two
			PlotlyBar barTwo = new()
			{
				X = projects.Select(p => p.Name).ToArray(),
				Y = projects.Select(async p => (await _projectService.GetProjectMembersByRoleAsync(p.Id, nameof(RolesEnum.Developer)))
																	 .Count).Select(c => c.Result).ToArray(),
				Name = "Developers",
				Type = "bar"
			};

			barData.Add(barOne);
			barData.Add(barTwo);

			plotlyData.Data = barData;

			return Json(plotlyData);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
