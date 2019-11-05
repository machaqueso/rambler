namespace Rambler.Models
{
    public class BotActionType
    {
        public static string Say = "Say";
        public static string PlayMedia = "Play media";
        public static string Custom = "Custom";

        public static string[] All =
        {
            Say,
            PlayMedia,
            Custom
        };
    }
}