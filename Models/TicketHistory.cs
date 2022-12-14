using System;
using System.ComponentModel;
namespace BugTracker.Models
{
    public class TicketHistory
    {
        // Note This Was Scaffold With Potential Bugs In Property, OldValue And NewValue
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Updated Item")]
        public string Property { get; set; }

        [DisplayName("Previous")]
        public string OldValue { get; set; }

        [DisplayName("Current")]
        public string NewValue { get; set; }

        [DisplayName("Date Modified")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Description Of Change")]
        public string Description { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }
        
        // Navigation Properties
        public virtual Ticket Ticket { get; set; }
        public virtual BugTrackerUser User { get; set; }
    }
}
