namespace onlineTicketing.Models
{
    public class TicketThreadViewModel
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public long SenderId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
        public string AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}