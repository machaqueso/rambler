namespace Rambler.Models
{
    public class ConfigurationSettingNames
    {
        public static string GoogleClientId = "Authentication:Google:ClientId";
        public static string GoogleClientSecret = "Authentication:Google:ClientSecret";
        public static string TwitchClientId = "Authentication:Twitch:ClientId";
        public static string TwitchClientSecret = "Authentication:Twitch:ClientSecret";
        public static string DiscordToken = "Authentication:Discord:Token";

        // Channel rules
        public static string ChannelTTSAuthorThreshold = "TTS Author Threshold";
        public static string ChannelOBSAuthorThreshold = "OBS Author Threshold";
        public static string ChannelReaderAuthorThreshold = "Reader Author Threshold";

        // Chat rule settings
        public static string ChatRulesMaxDuplicateWords = "Chat Rules Max Duplicate Words";
        public static string ChatRulesMaxDuplicateCharacters = "Chat Rules Max Duplicate Characters";
        public static string ChatRulesMaxWordLength = "Chat Rules Max Word Length";
        public static string ChatRulesMaxDuplicateMessages = "Chat Rules Max Duplicate Messages";
        public static string ChatRulesMaxDuplicateMessageTime = "Chat Rules Max Duplicate Message Time";

        public static string[] All =
        {
            GoogleClientId,
            GoogleClientSecret,
            TwitchClientId,
            TwitchClientSecret,
            DiscordToken,

            ChannelTTSAuthorThreshold,
            ChannelOBSAuthorThreshold,
            ChannelReaderAuthorThreshold,

            ChatRulesMaxDuplicateWords,
            ChatRulesMaxDuplicateCharacters,
            ChatRulesMaxWordLength,
            ChatRulesMaxDuplicateMessages,
            ChatRulesMaxDuplicateMessageTime
    };

    }
}