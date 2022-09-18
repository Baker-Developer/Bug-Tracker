using System.Security.Claims;
using System.Security.Principal;

namespace BugTracker.Extentions
{
    public static class IdentityExtentions
    {
        public static int? GetCompanyId(this IIdentity identity)
        {
            Claim claim = ((ClaimsIdentity)identity).FindFirst("CompanyId");
            // Ternary Operator (If/Else)
            return (claim != null) ? int.Parse(claim.Value) : null;
        }
    }
}
