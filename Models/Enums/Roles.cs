using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BugTracker.Models.Enums
{
    public enum Roles
    {
        [Display(Name = "Administrator")]
        Admin,
        [Display(Name = "Project Manager")]
        ProjectManager,
        [Display(Name = "Developer")]
        Developer,
        [Display(Name = "Submitter")]
        Submitter,
        [Display(Name = "Demo User")]
        DemoUser
    }
}
