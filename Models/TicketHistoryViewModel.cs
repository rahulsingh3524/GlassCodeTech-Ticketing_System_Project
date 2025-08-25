

namespace onlineTicketing.Models
    {
        public class TicketHistoryViewModel
        {
            public int Id { get; set; }
            public int TicketId { get; set; }
            public string Status { get; set; }
            public string ChangedByUsername { get; set; }
            public DateTime ChangedAt { get; set; }
            public string Note { get; set; }
        

    }
    }


