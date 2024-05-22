using System;
using System.Collections.Generic;

namespace BookReadingService.Models
{
    public partial class Account
    {
        public int AccountId { get; set; }
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Path { get; set; } = null!;
        public bool Gender { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool? Membership { get; set; }
        public int? MembershipId { get; set; }
        public int? Balance { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set;}

        public virtual Membership? Memberships { get; set; }
    }
}
