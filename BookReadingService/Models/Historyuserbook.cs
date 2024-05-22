using System;
using System.Collections.Generic;

namespace BookReadingService.Models
{
    public partial class Historyuserbook
    {
        public int HistoryuserbookId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? AccountId { get; set; }
        public int? BookId { get; set; }

        public virtual Category? Account { get; set; }
        public virtual Category? Book { get; set; }
    }
}
