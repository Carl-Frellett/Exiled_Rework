using System;

namespace DreamPlugin.Badge.Model
{
    public class PlayerSession
    {
        public BadgeAccount CurrentBadge { get; set; }
        public bool IsLoggedIn { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsAutoReconnected { get; set; }
    }
}