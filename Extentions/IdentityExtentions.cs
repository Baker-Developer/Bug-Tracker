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

        // Above ternary operator explanation

        //  int result;
        // if (claim != null)
        //{
            // result = int.Parse(claim.Value);
        //} else 
        //{
            //result = 0;
        //}

        //return result;



    }
}
