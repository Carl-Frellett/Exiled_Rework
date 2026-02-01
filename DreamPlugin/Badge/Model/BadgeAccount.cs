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
        public int ExpirationMonths { get; set; } = 0; // 0表示永久

        [JsonProperty("createTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [JsonProperty("lastUpdateTime")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        // 检查是否过期
        public bool IsExpired()
        {
            if (ExpirationMonths == 0) return false; // 永久不过期

            // 如果创建时间是默认值，说明是旧数据，设为永久
            if (CreateTime == default(DateTime))
            {
                CreateTime = DateTime.Now;
                ExpirationMonths = 0;
                return false;
            }

            return DateTime.Now > CreateTime.AddMonths(ExpirationMonths);
        }

        // 获取过期时间
        public DateTime GetExpirationDate()
        {
            if (ExpirationMonths == 0) return DateTime.MaxValue;

            // 如果创建时间是默认值，说明是旧数据，设为永久
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