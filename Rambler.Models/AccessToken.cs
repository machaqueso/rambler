using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rambler.Models
{
    public class AccessToken
    {
        public int Id { get; set; }
        public string ApiSource { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public DateTime ExpirationDate { get; set; }

        [NotMapped]
        public string Status
        {
            get
            {
                if (DateTime.UtcNow > ExpirationDate)
                {
                    return AccessTokenStatus.Expired;
                }

                return AccessTokenStatus.Ok;
            }
        }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        [NotMapped] public bool HasRefreshToken => !string.IsNullOrEmpty(refresh_token);
    }
}