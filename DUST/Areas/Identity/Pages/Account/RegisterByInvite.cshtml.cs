using DUST.Models;
using DUST.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace DUST.Areas.Identity.Pages.Account
{
    public class RegisterByInviteModel : RegisterModel
    {
        public RegisterByInviteModel(
            UserManager<DUSTUser> userManager,
            SignInManager<DUSTUser> signInManager,
            ILogger<RegisterModel> logger,
            IInviteService inviteService,
            IEmailSender emailSender,
            ICompanyInfoService companyService
            ) : base(userManager, signInManager, logger, emailSender, inviteService, companyService)
        {}
    }
}
