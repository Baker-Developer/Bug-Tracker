using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BugTracker.Models.Enums
{
    public enum BugTrackerTicketType
    {
        [Display(Name = "New Development")]
        NewDevelopment,
       
        WorkTask,
        
        Defect,
      
        ChangeRequest,
        
        Enhancement,
       
        GeneralTask
    }
}
