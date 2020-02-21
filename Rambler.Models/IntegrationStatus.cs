using System.Collections;
using System.Collections.Generic;

namespace Rambler.Models
{
    public class IntegrationStatus
    {
        public static string Disabled = "Disabled";
        public static string Stopped = "Stopped";
        public static string NotConfigured = "Not Configured";
        public static string Forbidden = "Forbidden";
        public static string Connected = "Connected";
        public static string Offline = "Offline";
        public static string Error = "Error";
        public static string Starting = "Starting";
        public static string Stopping = "Stopping";

        public static IEnumerable<string> All = new[]
        {
            Disabled,
            Stopped,
            NotConfigured,
            Forbidden,
            Connected,
            Offline,
            Error,
            Starting,
            Stopping
        };
    }
}