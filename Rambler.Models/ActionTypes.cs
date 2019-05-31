namespace Rambler.Models
{
    public class ActionTypes
    {
        public static string Upvote = "Upvote";
        public static string Downvote = "Downvote";
        public static string Whitelist = "Whitelist";
        public static string Ignore = "Ignore";
        public static string Blacklist = "Blacklist";
        public static string Ban = "Ban";

        public static string[] All =
        {
            Upvote,
            Downvote,
            Whitelist,
            Ignore,
            Blacklist,
            Ban
        };
    }
}