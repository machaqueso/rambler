using System;
using System.Collections.Generic;

namespace Rambler.Models
{
    public class PageInfo
    {
        public int totalResults { get; set; }
        public int resultsPerPage { get; set; }
    }

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

    public class LiveBroadcast
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
        public IEnumerable<LiveBroadcast> items { get; set; }
    }
    public class LiveChatMessageList
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public int pollingIntervalMillis { get; set; }

        public PageInfo pageInfo { get; set; }
        public IEnumerable<LiveChatMessage> items { get; set; }
    }

    public class LiveChatMessage
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public LiveChatMessageSnippet snippet { get; set; }
    }

    public class LiveChatMessageSnippet
    {
        public string type { get; set; }
        public string liveChatId { get; set; }
        public string authorChannelId { get; set; }
        public DateTime publishedAt { get; set; }
        public bool hasDisplayContent { get; set; }
        public string displayMessage { get; set; }

        public TextMessageDetails textMessageDetails { get; set; }
    }

    public class TextMessageDetails
    {
        public string messageText { get; set; }
    }
}