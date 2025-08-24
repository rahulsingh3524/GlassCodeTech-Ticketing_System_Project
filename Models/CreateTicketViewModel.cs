//namespace GlassCodeTech_Ticketing_System_Project.Models
//{
//    public class CreateTicketViewModel
//    {
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace onlineTicketing.Models
{
    public class CreateTicketViewModel
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Description { get; set; }
        public string Category { get; set; }
        public int Priority { get; set; }

        // This property will hold the uploaded file. IFormFile is the ASP.NET Core interface for files.
        public IFormFile Attachment { get; set; }
    }
}
