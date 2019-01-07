using System;
using System.Collections.Generic;

namespace Rambler.Web.Models.Youtube.LiveBroadcast
{
    public class Snippet
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

    public class Item
    {
        public string kind { get; set; }
        public string etag { get; set; }

        public string id { get; set; }
        public Snippet snippet { get; set; }
    }

    public class List
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public PageInfo pageInfo { get; set; }
        public IEnumerable<Item> items { get; set; }
    }

}