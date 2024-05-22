using System;
using System.Collections.Generic;

namespace AuthService.Models
{
    public partial class Account
    {
        public int AccountId { get; set; }
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Path { get; set; } = null!;
        public bool? Gender { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool? Membership { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
