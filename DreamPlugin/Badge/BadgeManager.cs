using DreamPlugin.Badge.Controller;
using DreamPlugin.Badge.Model;
using Newtonsoft.Json;
using RExiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DreamPlugin.Badge
{
    public class BadgeManager
    {
        private Dictionary<string, BadgeAccount> badges = new Dictionary<string, BadgeAccount>();
        private Dictionary<Player, PlayerSession> playerSessions = new Dictionary<Player, PlayerSession>();
        private string dataPath;

        public BadgeManager()
        {
            dataPath = Plugin.plugin.Config.BadgeDataPath;
        }

        public void LoadBadges()
        {
            try
            {
                if (File.Exists(dataPath))
                {
                    string json = File.ReadAllText(dataPath);
                    var loadedBadges = JsonConvert.DeserializeObject<List<BadgeAccount>>(json);

                    badges = loadedBadges
                        .Where(b => !b.IsExpired())
                        .ToDictionary(b => b.UserId, b => b);

                    SaveBadges();

                    Log.Info($"已加载 {badges.Count} 个有效称号");
                }
                else
                {
                    badges = new Dictionary<string, BadgeAccount>();
                    Log.Info("未找到称号数据文件，将创建新文件");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"加载称号数据失败: {ex}");
            }
        }

        public void SaveBadges()
        {
            try
            {
                var validBadges = badges.Values.Where(b => !b.IsExpired()).ToList();
                string json = JsonConvert.SerializeObject(validBadges, Formatting.Indented);

                string directory = Path.GetDirectoryName(dataPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(dataPath, json);
                Log.Info("称号数据已保存");
            }
            catch (Exception ex)
            {
                Log.Error($"保存称号数据失败: {ex}");
            }
        }

        public bool RegisterBadge(string userId, BadgeType type, string content, string color = null, int expirationMonths = 0)
        {
            bool isUpdate = badges.ContainsKey(userId);
            if (isUpdate)
            {
                var existing = badges[userId];
                existing.BadgeType = type;
                existing.BadgeContent = content;
                existing.BadgeColor = color;
                existing.ExpirationMonths = expirationMonths;
                existing.LastUpdateTime = DateTime.Now;
                Log.Info($"更新称号: {userId}");
            }
            else
            {
                var newBadge = new BadgeAccount
                {
                    UserId = userId,
                    BadgeType = type,
                    BadgeContent = content,
                    BadgeColor = color,
                    ExpirationMonths = expirationMonths,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };
                badges[userId] = newBadge;
                Log.Info($"创建新称号: {userId}");
            }
            SaveBadges();
            return true;
        }

        public bool DeleteBadge(string userId)
        {
            if (!badges.ContainsKey(userId)) return false;
            badges.Remove(userId);
            SaveBadges();
            return true;
        }
        public void ApplyBadgeToPlayer(Player player)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) return;

            if (badges.TryGetValue(player.UserId, out var badge) && !badge.IsExpired())
            {
                ApplyBadgeDirectly(player, badge);
            }
            else
            {
                CleanPlayerBadge(player);
            }
        }

        public void CleanPlayerBadge(Player player)
        {
            if (player == null) return;

            var rainbow = player.GameObject.GetComponent<RainbowTagController>();
            var dynamic = player.GameObject.GetComponent<DynamicBadgeController>();

            if (rainbow != null) UnityEngine.Object.Destroy(rainbow);
            if (dynamic != null) UnityEngine.Object.Destroy(dynamic);

            player.RankName = null;
            player.RankColor = "default";
        }
        public void ApplyBadgeDirectly(Player player, BadgeAccount badge)
        {
            if (badge == null) return;

            switch (badge.BadgeType)
            {
                case BadgeType.Simple:
                    player.RankName = badge.BadgeContent;
                    player.RankColor = badge.BadgeColor ?? "red";
                    break;
                case BadgeType.Rainbow:
                    player.RankName = badge.BadgeContent;
                    var rainbowController = player.GameObject.AddComponent<RainbowTagController>();
                    rainbowController.Initialize(player, badge.RainbowColors);
                    break;
                case BadgeType.SimpleDynamic:
                case BadgeType.RainbowDynamic:
                    var dynamicController = player.GameObject.AddComponent<DynamicBadgeController>();
                    dynamicController.Initialize(player, badge.BadgeContent, badge.BadgeColor ?? "red",
                        badge.BadgeType, badge.RainbowColors);
                    break;
            }
        }

        private void ApplyBadge(Player player, BadgeAccount badge)
        {
            RemoveBadge(player);

            switch (badge.BadgeType)
            {
                case BadgeType.Simple:
                    ApplySimpleBadge(player, badge);
                    break;
                case BadgeType.Rainbow:
                    ApplyRainbowBadge(player, badge);
                    break;
                case BadgeType.SimpleDynamic:
                    ApplySimpleDynamicBadge(player, badge);
                    break;
                case BadgeType.RainbowDynamic:
                    ApplyRainbowDynamicBadge(player, badge);
                    break;
            }
        }

        private void ApplySimpleBadge(Player player, BadgeAccount badge)
        {
            player.RankName = badge.BadgeContent;
            player.RankColor = badge.BadgeColor ?? "red";
        }

        private void ApplyRainbowBadge(Player player, BadgeAccount badge)
        {
            player.RankName = badge.BadgeContent;

            var controller = player.GameObject.AddComponent<RainbowTagController>();
            controller.Initialize(player, badge.RainbowColors);
        }

        private void ApplySimpleDynamicBadge(Player player, BadgeAccount badge)
        {
            var controller = player.GameObject.AddComponent<DynamicBadgeController>();
            controller.Initialize(player, badge.BadgeContent, badge.BadgeColor ?? "red",
                BadgeType.SimpleDynamic, badge.RainbowColors);
        }

        private void ApplyRainbowDynamicBadge(Player player, BadgeAccount badge)
        {
            var controller = player.GameObject.AddComponent<DynamicBadgeController>();
            controller.Initialize(player, badge.BadgeContent, badge.BadgeColor ?? "red",
                BadgeType.RainbowDynamic, badge.RainbowColors);
        }

        private void RemoveBadge(Player player)
        {
            var rainbowController = player.GameObject.GetComponent<RainbowTagController>();
            if (rainbowController != null)
                UnityEngine.Object.Destroy(rainbowController);

            var dynamicController = player.GameObject.GetComponent<DynamicBadgeController>();
            if (dynamicController != null)
                UnityEngine.Object.Destroy(dynamicController);
        }

        public void OnPlayerLeft(Player player)
        {
            if (!string.IsNullOrEmpty(player.UserId))
            {
                CleanPlayerBadge(player);
            }
        }

        public string GetAccountExpirationInfo(string userId)
        {
            if (!badges.TryGetValue(userId, out var badge))
                return "无称号记录";

            if (badge.IsExpired())
                return "已过期（已自动清理）";

            if (badge.ExpirationMonths == 0)
                return "永久有效";

            var daysLeft = (badge.GetExpirationDate() - DateTime.Now).Days;
            return $"{daysLeft} 天后过期（{badge.GetExpirationDate():yyyy-MM-dd}）";
        }
    }
}