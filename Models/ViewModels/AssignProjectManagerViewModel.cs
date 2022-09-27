using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class AssignProjectManagerViewModel
    {
        public Project Project { get; set; }
        public SelectList ProjectManagerList { get; set; }
        public string ProjectManagerId { get; set; }
    } 
}
