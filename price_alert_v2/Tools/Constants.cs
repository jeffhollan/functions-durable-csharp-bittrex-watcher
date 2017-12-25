using System;
using Twilio.Types;

namespace price_alert_v2
{
    internal class Constants
    {
        internal static string StorageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        // Bittrex
        internal static string GetMarketsUrl = "https://bittrex.com/api/v1.1/public/getmarkets";
        internal static string TickerUrl = "https://bittrex.com/api/v1.1/public/getticker";
        
        // Twilio SMS
        internal static string Twilio_accountSid = Environment.GetEnvironmentVariable("Twilio_accountSid");
        internal static string Twilio_authToken = Environment.GetEnvironmentVariable("Twilio_authToken");
        internal static string Twilio_number = Environment.GetEnvironmentVariable("Twilio_number");

        // Config
        public static string MaxDuration = Environment.GetEnvironmentVariable("Config_maxMinutes") ?? "240";
        public static string DelayInterval = Environment.GetEnvironmentVariable("Config_delayMinutes") ?? "15";
    }
}