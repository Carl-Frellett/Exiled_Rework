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

                    bool needsUpgrade = false;

                    foreach (var badge in loadedBadges)
                    {
                        if (badge.CreateTime == default(DateTime))
                        {
                            badge.CreateTime = DateTime.Now;
                            badge.LastUpdateTime = DateTime.Now;

                            if (badge.ExpirationMonths == 0)
                            {
                                badge.ExpirationMonths = 0;
                            }

                            needsUpgrade = true;
                            Log.Info($"升级旧格式称号账号: {badge.Account} (设为永久)");
                        }
                    }

                    badges = loadedBadges
                        .Where(b => !b.IsExpired())
                        .ToDictionary(b => b.Account, b => b);

                    if (loadedBadges.Count != badges.Count || needsUpgrade)
                    {
                        SaveBadges();
                        if (loadedBadges.Count != badges.Count)
                        {
                            Log.Info($"已清理 {loadedBadges.Count - badges.Count} 个过期称号账号");
                        }
                        if (needsUpgrade)
                        {
                            Log.Info("已升级旧格式称号数据");
                        }
                    }

                    Log.Info($"已加载 {badges.Count} 个称号账号");
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

        public bool RegisterBadge(string account, string password, BadgeType type, string content, string color = null, int expirationMonths = 0)
        {
            bool isUpdate = badges.ContainsKey(account);

            if (isUpdate)
            {
                var existingBadge = badges[account];
                existingBadge.Password = password;
                existingBadge.BadgeType = type;
                existingBadge.BadgeContent = content;
                existingBadge.BadgeColor = color;
                existingBadge.ExpirationMonths = expirationMonths;
                existingBadge.LastUpdateTime = DateTime.Now;

                Log.Info($"已更新称号账号: {account}, 有效期: {(expirationMonths == 0 ? "永久" : $"{expirationMonths}个月")}");
            }
            else
            {
                var newBadge = new BadgeAccount
                {
                    Account = account,
                    Password = password,
                    BadgeType = type,
                    BadgeContent = content,
                    BadgeColor = color,
                    ExpirationMonths = expirationMonths,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };

                badges[account] = newBadge;
                Log.Info($"已创建新称号账号: {account}, 有效期: {(expirationMonths == 0 ? "永久" : $"{expirationMonths}个月")}");
            }

            SaveBadges();
            return true;
        }

        public bool DeleteBadge(string account)
        {
            if (!badges.ContainsKey(account))
                return false;

            badges.Remove(account);
            SaveBadges();
            return true;
        }

        public BadgeAccount Login(Player player, string account, string password)
        {
            if (!badges.TryGetValue(account, out var badge) || badge.Password != password)
                return null;

            if (badge.IsExpired())
            {
                Log.Info($"称号账号 {account} 已过期，自动删除");
                badges.Remove(account);
                SaveBadges();
                return null;
            }

            if (playerSessions.ContainsKey(player))
            {
                RemoveBadge(player);
            }

            var session = new PlayerSession
            {
                CurrentBadge = badge,
                IsLoggedIn = true,
                LoginTime = DateTime.Now
            };

            playerSessions[player] = session;
            ApplyBadge(player, badge);

            badge.LastUpdateTime = DateTime.Now;
            SaveBadges();

            return badge;
        }

        public bool IsPlayerLoggedIn(Player player)
        {
            return playerSessions.ContainsKey(player) && playerSessions[player].IsLoggedIn;
        }

        public BadgeAccount GetPlayerBadge(Player player)
        {
            return playerSessions.TryGetValue(player, out var session) ? session.CurrentBadge : null;
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
                    // 彩虹称号需要控制器
                    var rainbowController = player.GameObject.AddComponent<RainbowTagController>();
                    rainbowController.Initialize(player, badge.RainbowColors);
                    break;
                case BadgeType.SimpleDynamic:
                case BadgeType.RainbowDynamic:
                    // 动态称号需要控制器
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
            // 移除彩虹标签控制器
            var rainbowController = player.GameObject.GetComponent<RainbowTagController>();
            if (rainbowController != null)
                UnityEngine.Object.Destroy(rainbowController);

            // 移除动态称号控制器
            var dynamicController = player.GameObject.GetComponent<DynamicBadgeController>();
            if (dynamicController != null)
                UnityEngine.Object.Destroy(dynamicController);

            // 注意：不要在这里重置玩家的RankName和RankColor
            // 因为单色称号玩家的称号可能会被错误重置
            // 改为在各个控制器的OnDestroy中处理
        }

        // 添加玩家断开连接时的清理方法
        public void OnPlayerLeft(Player player)
        {
            if (playerSessions.ContainsKey(player))
            {
                // 在玩家离开时正确重置其称号
                player.RankName = null;
                player.RankColor = "default";
                playerSessions.Remove(player);
            }
        }

        // 新增方法：获取账号过期信息
        public string GetAccountExpirationInfo(string account)
        {
            if (!badges.TryGetValue(account, out var badge))
                return "账号不存在";

            if (badge.IsExpired())
                return "已过期";

            if (badge.ExpirationMonths == 0)
                return "永久";

            var expirationDate = badge.GetExpirationDate();
            var daysLeft = (expirationDate - DateTime.Now).Days;
            return $"{daysLeft}天后过期 ({expirationDate:yyyy-MM-dd})";
        }

        // 新增方法：获取所有旧格式账号并升级
        public int UpgradeLegacyBadges()
        {
            int upgradedCount = 0;

            foreach (var badge in badges.Values)
            {
                // 如果创建时间为默认值，说明是旧格式数据
                if (badge.CreateTime == default(DateTime))
                {
                    badge.CreateTime = DateTime.Now;
                    badge.LastUpdateTime = DateTime.Now;

                    // 旧数据默认设为永久
                    if (badge.ExpirationMonths == 0)
                    {
                        badge.ExpirationMonths = 0; // 0表示永久
                    }

                    upgradedCount++;
                    Log.Info($"升级旧格式称号账号: {badge.Account} (设为永久)");
                }
            }

            if (upgradedCount > 0)
            {
                SaveBadges();
                Log.Info($"已升级 {upgradedCount} 个旧格式称号账号");
            }

            return upgradedCount;
        }
    }
}