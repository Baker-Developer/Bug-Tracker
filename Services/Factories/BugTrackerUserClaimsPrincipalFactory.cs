using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BugTracker.Services.Factories
{
    public class BugTrackerUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BugTrackerUser, IdentityRole>
    {
        public BugTrackerUserClaimsPrincipalFactory(UserManager<BugTrackerUser> userManager,
                                                    RoleManager<IdentityRole> roleManager, 
                                                    IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {


        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BugTrackerUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            

            return identity;
        }
    }
}
