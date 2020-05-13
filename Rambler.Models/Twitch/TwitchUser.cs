using System;

namespace Rambler.Models.Twitch
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
        public int id { get; set; }
        public bool email { get; set; }
        public bool push { get; set; }
    }

    public class TwitchGetUsersResponse
    {
        public ulong _total { get; set; }
        public TwitchUser[] users { get; set; }
    }

    public class TwitchLink
    {
        public string self { get; set; }
    }

    public class TwitchEmoticonImage
    {
        public int width { get; set; }
        public int height { get; set; }
        public string url { get; set; }
        public string emoticon_set { get; set; }
    }

    public class TwitchEmoticon
    {
        public ulong id { get; set; }
        public string regex { get; set; }
        public TwitchEmoticonImage images { get; set; }
    }

    public class TwitchChatEmoticonsResponse
    {
        public TwitchLink _link { get; set; }
        public TwitchEmoticon[] emoticons { get; set; }
    }

    public class TwitchUserEmoticonSetResponse
    {
        public dynamic emoticon_sets { get; set; }
    }
}