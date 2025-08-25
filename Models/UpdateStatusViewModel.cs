//// In onlineTicketing.Models

//using Microsoft.AspNetCore.Mvc.Rendering;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using onlineTicketing.Models;

//namespace onlineTicketing.Models
//{

//    public class UpdateStatusViewModel
//    {
//        public int TicketId { get; set; }
//        public string TicketSubject { get; set; }
//        public string CurrentStatus { get; set; }

//        //[Required]
//        //[Display(Name = "New Status")]
//        //public string NewStatus { get; set; }

//        //[Display(Name = "Notes")]
//        //public string Note { get; set; }

//        public string NewStatus { get; set; }


//        public string Note { get; set; }


//        public IEnumerable<SelectListItem> StatusOptions { get; set; }
//    }
//}


using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace onlineTicketing.Models
{

    public class UpdateStatusViewModel
    {
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Please select a status.")]
        public string NewStatus { get; set; }

        public string Note { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> StatusOptions { get; set; }
    }
}
