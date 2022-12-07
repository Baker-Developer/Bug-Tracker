using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

/*
  Roles Explained:
  ADMIN: FULL CONTROL OVER APPLICATION
  DEVELOPER: CAN CHANGE THE STATUS OF A TICKET
  SUBMITTER: CAN MAKE NEW TICKETS FOR A PROJECT
  PROJECT MANAGER:CAN ASSIGN DEVELOPERS AND USERS TO PROJECTS
*/
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
