//namespace GlassCodeTech_Ticketing_System_Project.Models
//{
//    public class AssignTicketModel
//    {
//    }
//}

namespace onlineTicketing.Models
{
    public class AssignTicketModel
    {
        public int Id { get; set; }
        public string TicketId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public sbyte AssignedTo { get; set; }
    }
}

