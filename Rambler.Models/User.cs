using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rambler.Models
{
    public class User
    {
        public int Id { get; set; }
        public virtual ICollection<AccessToken> AccessTokens { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int FailedLogins { get; set; }
        public bool IsLocked { get; set; }
        public bool MustChangePassword { get; set; }

        [NotMapped] public string Password { get; set; }
        [NotMapped] public string ConfirmPassword { get; set; }
    }
}