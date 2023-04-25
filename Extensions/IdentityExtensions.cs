using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace DUST.Extensions
{
    public static class IdentityExtensions
    {
        // Extension method that lets us use this method as if it were
        // an instance method of type IIdentity
        public static int? GetCompanyId(this IIdentity identity)
        {
            Claim claim = ((ClaimsIdentity)identity).FindFirst("CompanyId");
            return (claim != null) ? int.Parse(claim.Value) : null;
        }
    }
}
