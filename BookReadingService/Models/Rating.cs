namespace BookReadingService.Models
{
    public class Rating
    {
        public int RatingId { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Status { get; set; }

        public int? BookId { get; set; }
        public int? AccountId { get; set; }

        public virtual Book? Book { get; set; }
        public virtual Account? Account { get; set; }

    }
}