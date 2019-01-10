using System;
using System.Collections.Generic;

namespace Rambler.Web.Models.Youtube.LiveBroadcast
{
    public class LiveBroadcastSnippet
    {
        public DateTime publishedAt { get; set; }
        public string channelId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime scheduledStartTime { get; set; }
        public DateTime actualStartTime { get; set; }
        public bool isDefaultBroadcast { get; set; }
        public string liveChatId { get; set; }
    }

    public class LiveBroadcastItem
    {
        public string kind { get; set; }
        public string etag { get; set; }

        public string id { get; set; }
        public LiveBroadcastSnippet snippet { get; set; }
    }

    public class LiveBroadcastList
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public PageInfo pageInfo { get; set; }
        public IEnumerable<LiveBroadcastItem> items { get; set; }
    }

}