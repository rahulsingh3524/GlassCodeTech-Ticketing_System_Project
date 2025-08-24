//namespace GlassCodeTech_Ticketing_System_Project.Models
//{
//    public class TicketDetailsViewModel
//    {
//    }
//}



using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace onlineTicketing.Models
{
    public class TicketDetailsViewModel
    {
        public TicketViewModel Ticket { get; set; }
        public List<TicketThreadViewModel> Threads { get; set; }
        public AddMessageViewModel NewMessage { get; set; }
        public List<SelectListItem> StatusOptions { get; set; }
        public List<SelectListItem> Supporters { get; set; }

        public List<SelectListItem> PriorityOptions { get; set; }

        public bool IsSupporter { get; set; }
        public List<TicketHistoryViewModel> History { get; set; } // <-- NEW
    }
}

