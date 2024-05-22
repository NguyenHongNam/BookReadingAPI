using System;
using System.Collections.Generic;

namespace BookReadingService.Models
{
    public partial class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Publisher { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Imgsrc { get; set; }
        public int Views { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public bool? ForMembership { get; set; }
        public int? price { get; set; }
        public int? CategoryId { get; set; }
        public int? AuthorId { get; set; }
        public int? Likes { get; set; }
        public int? Pages { get; set; }
        public virtual Category? Category { get; set; }
        public virtual Author? Author { get; set; }
    }
}
