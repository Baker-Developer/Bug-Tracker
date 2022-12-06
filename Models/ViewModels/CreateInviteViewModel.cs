using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;

namespace BugTracker.Models.ViewModels
{
    public class CreateInviteViewModel
    {
        public Invite Invite { get; set; }
        public SelectList Projects { get; set; }
        public int SelectedProjectId { get; set; }

    }
}
