using System;
using System.Collections.Generic;

namespace Rambler.Web.Models.Youtube.LiveChat
{
    public class MessageList
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public int pollingIntervalMillis { get; set; }

        public PageInfo pageInfo { get; set; }
        public IEnumerable<Message> items { get; set; }
    }

    public class Message
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public Snippet snippet { get; set; }
        public AuthorDetails AuthorDetails { get; set; }
    }

    public class Snippet
    {
        public string type { get; set; }
        public string liveChatId { get; set; }
        public string authorChannelId { get; set; }
        public DateTime publishedAt { get; set; }
        public bool hasDisplayContent { get; set; }
        public string displayMessage { get; set; }

        public FanFundingEventDetails fanFundingEventDetails { get; set; }
        public TextMessageDetails textMessageDetails { get; set; }
        public MessageDeletedDetails messageDeletedDetails { get; set; }
        public UserBannedDetails userBannedDetails { get; set; }
        public SuperChatDetails superChatDetails { get; set; }
    }

    public class SuperChatDetails
    {
        public ulong amountMicros { get; set; }
        public string currency { get; set; }
        public string amountDisplayString { get; set; }
        public string userComment { get; set; }
        public uint tier { get; set; }
    }

    public class UserBannedDetails
    {
        public BannedUserDetails bannedUserDetails { get; set; }
        public string banType { get; set; }
        public string banDurationSeconds { get; set; }
    }

    public class BannedUserDetails
    {
        public string channelId { get; set; }
        public string channelUrl { get; set; }
        public string displayName { get; set; }
        public string profileImageUrl { get; set; }
    }

    public class MessageDeletedDetails
    {
        public string deletedMessageId { get; set; }
    }

    public class FanFundingEventDetails
    {
        public ulong amountMicros { get; set; }
        public string currency { get; set; }
        public string amountDisplayString { get; set; }
        public string userComment { get; set; }
    }

    public class TextMessageDetails
    {
        public string messageText { get; set; }
    }

    public class AuthorDetails
    {
        public string channelId { get; set; }
        public string channelUrl { get; set; }
        public string displayName { get; set; }
        public string profileImageUrl { get; set; }
        public bool isVerified { get; set; }
        public bool isChatOwner { get; set; }
        public bool isChatSponsor { get; set; }
        public bool isChatModerator { get; set; }
    }
}