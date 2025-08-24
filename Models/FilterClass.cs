//namespace GlassCodeTech_Ticketing_System_Project.Models
//{
//    public class FilterClass
//    {
//    }
//}

using Microsoft.AspNetCore.Mvc.Rendering;

namespace onlineTicketing.Models
{
    // You may want to add this to your Models folder
    public class TicketListViewModel
    {
        public List<TicketViewModel> Tickets { get; set; }
        public IEnumerable<SelectListItem> StatusOptions { get; set; }
        public IEnumerable<SelectListItem> PriorityOptions { get; set; }
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public int? SelectedStatus { get; set; }
        public int? SelectedPriority { get; set; }
        public string SelectedCategory { get; set; }
    }
}

