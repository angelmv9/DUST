using DUST.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DUST.Services.Factories
{
    public class DUSTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<DUSTUser, IdentityRole>
    {

        public DUSTUserClaimsPrincipalFactory(UserManager<DUSTUser> userManager,
                                      RoleManager<IdentityRole> roleManager,
                                      IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        // Optains the companyId of the user during login
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(DUSTUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            return identity;
        }
    }
}
