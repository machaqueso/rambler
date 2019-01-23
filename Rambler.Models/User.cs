using System.Collections.Generic;

namespace Rambler.Models
{
    public class User
    {
        public int Id { get; set; }
        public virtual ICollection<AccessToken> AccessTokens { get; set; }
    }
}