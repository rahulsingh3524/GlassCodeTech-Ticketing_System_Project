//namespace GlassCodeTech_Ticketing_System_Project.Models
//{
//    public class statusupdate
//    {
//    }
//}

using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace onlineTicketing.Models
{
    public class statusupdate
    {
        public int TicketId { get; set; }
        public string NewStatus { get; set; }

        public string Note { get; set; }

        public IEnumerable<SelectListItem> StatusOptions { get; set; }
    }
}

