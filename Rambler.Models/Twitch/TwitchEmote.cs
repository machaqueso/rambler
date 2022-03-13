using System.Collections.Generic;

namespace Rambler.Models.Twitch
{
    // TwitchEmote replace TwitchEmoticon from older APIs
    public class TwitchEmote
    {
        public IEnumerable<TwitchEmoteData> data { get; set; }
        public string template { get; set; }
    }

    public class TwitchEmoteData
    {
        public string id { get; set; }
        public string name { get; set; }
        public TwitchEmoteImages images { get; set; }
        public IEnumerable<string> format { get; set; }
        public IEnumerable<string> scale { get; set; }
        public IEnumerable<string> theme_mode { get; set; }
    }

    public class TwitchEmoteImages
    {
        public string url_1x { get; set; }
        public string url_2x { get; set; }
        public string url_4x { get; set; }
    }
}