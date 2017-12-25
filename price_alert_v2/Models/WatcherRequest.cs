using System;

namespace price_alert_v2
{
    public class WatcherRequest
    {
        public string market { get; set; }
        public double threshold { get; set; }
        public TimeSpan maxDuration { get; set; }
        public string[] args { get; set; }
        public string phone { get; set; }
    }
}