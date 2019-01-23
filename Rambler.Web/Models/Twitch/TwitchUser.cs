using System;

namespace Rambler.Web.Models.Twitch
{
    public class TwitchUser
    {
        public int id { get; set; }
        public ulong _id { get; set; }
        public string bio { get; set; }
        public DateTime created_at { get; set; }
        public string display_name { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
        public string logo { get; set; }
        public string name { get; set; }
        public TwitchNotifications notifications { get; set; }
        public bool partnered { get; set; }
        public bool twitter_connected { get; set; }
        public string type { get; set; }
        public DateTime? updated_at { get; set; }
    }

    public class TwitchNotifications
    {
        public bool email { get; set; }
        public bool push { get; set; }
    }
}