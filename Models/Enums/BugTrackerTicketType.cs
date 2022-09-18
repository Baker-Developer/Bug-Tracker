using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BugTracker.Models.Enums
{
    public enum BugTrackerTicketType
    {
        [Display(Name = "New Development")]
        NewDevelopment,
        [Display(Name = "Work Task")]
        WorkTask,
        [Display(Name = "Defect")]
        Defect,
        [Display(Name = "Change Request")]
        ChangeRequest,
        [Display(Name = "Enhancement")]
        Enhancement,
        [Display(Name = "General Task")]
        GeneralTask
    }
}
