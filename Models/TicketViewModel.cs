
using System;
using System.ComponentModel.DataAnnotations;

namespace onlineTicketing.Models
{
    public class TicketViewModel
    {
        public int Id { get; set; }
        public string TicketId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        // Use a nullable type since a ticket might not be assigned
        public string AssignedTo { get; set; }

        // This new property will hold the supporter's name
        [Display(Name = "Assigned To")]
        public string AssignedToName { get; set; }
        public DateTime? Deadline { get; set; }
    }
}