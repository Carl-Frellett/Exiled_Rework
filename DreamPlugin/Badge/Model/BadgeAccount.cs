using Newtonsoft.Json;
using System;

namespace DreamPlugin.Badge.Model
{
    public class BadgeAccount
    {
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("badgeType")]
        public BadgeType BadgeType { get; set; }

        [JsonProperty("badgeContent")]
        public string BadgeContent { get; set; }

        [JsonProperty("badgeColor")]
        public string BadgeColor { get; set; }

        [JsonProperty("rainbowColors")]
        public string[] RainbowColors { get; set; } = new[]
        {
            "pink", "red", "brown", "silver", "light_green", "crimson",
            "cyan", "aqua", "deep_pink", "tomato", "yellow", "magenta",
            "blue_green", "orange", "lime", "green", "emerald", "carmine",
            "nickel", "mint", "army_green", "pumpkin"
        };

        [JsonProperty("expirationMonths")]
        public int ExpirationMonths { get; set; } = 0;

        [JsonProperty("createTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [JsonProperty("lastUpdateTime")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        public bool IsExpired()
        {
            if (ExpirationMonths == 0) return false;

            if (CreateTime == default(DateTime))
            {
                CreateTime = DateTime.Now;
                ExpirationMonths = 0;
                return false;
            }

            return DateTime.Now > CreateTime.AddMonths(ExpirationMonths);
        }

        public DateTime GetExpirationDate()
        {
            if (ExpirationMonths == 0) return DateTime.MaxValue;

            if (CreateTime == default(DateTime))
            {
                CreateTime = DateTime.Now;
                ExpirationMonths = 0;
                return DateTime.MaxValue;
            }

            return CreateTime.AddMonths(ExpirationMonths);
        }
    }
}