//namespace GlassCodeTech_Ticketing_System_Project.Models
//{
//    public class AddMessageViewModel
//    {
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace onlineTicketing.Models
{
    public class AddMessageViewModel
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 1000 characters.")]
        public string Message { get; set; }

        public IFormFile Attachment { get; set; }
    }
}
