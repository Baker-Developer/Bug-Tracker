using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BugTracker.Models.Enums
{
    public enum BugTrackerTicketType
    {
       
        NewDevelopment,
       
        WorkTask,
        
        Defect,
      
        ChangeRequest,
        
        Enhancement,
       
        GeneralTask
    }
}
