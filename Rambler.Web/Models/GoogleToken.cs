using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rambler.Web.Models
{

    public class GoogleToken
    {
        private int _expiresIn;

        public int Id { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in
        {
            get { return _expiresIn; }
            set
            {
                _expiresIn = value;
                this.ExpirationDate = DateTime.Now.AddSeconds(_expiresIn);
            }
        }
        public string refresh_token { get; set; }
        public DateTime ExpirationDate { get; set; }

        public int UserId {get;set;}
        [ForeignKey("UserId")]
        public virtual User User{get;set;}
    }
}