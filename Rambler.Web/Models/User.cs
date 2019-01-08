using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rambler.Web.Models
{
    public class User
    {
        public int Id { get; set; }

        public int? GoogleTokenId { get; set; }
        [ForeignKey("GoogleTokenId")] public virtual GoogleToken GoogleToken { get; set; }

        public virtual ICollection<AccessToken> AccessTokens { get; set; }
    }
}
