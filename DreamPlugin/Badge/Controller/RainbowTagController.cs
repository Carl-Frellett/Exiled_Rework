using RExiled.API.Features;
using System.Collections;
using UnityEngine;

namespace DreamPlugin.Badge.Controller
{
    public class RainbowTagController : MonoBehaviour
    {
        private Player player;
        private string[] colors;
        private float interval;
        private float timer;
        private int currentIndex;

        public void Initialize(Player targetPlayer, string[] colorArray)
        {
            player = targetPlayer;
            colors = colorArray ?? new[] { "red", "blue", "green", "yellow" };
            interval = Plugin.plugin.Config.ColorChangeInterval;
            currentIndex = 0;

            StartCoroutine(ColorChangeRoutine());
        }

        private IEnumerator ColorChangeRoutine()
        {
            while (player != null)
            {
                if (timer >= interval)
                {
                    UpdateColor();
                    timer = 0f;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            Destroy(this);
        }

        private void UpdateColor()
        {
            if (colors.Length == 0) return;

            player.RankColor = colors[currentIndex];
            currentIndex = (currentIndex + 1) % colors.Length;
        }

        void OnDestroy()
        {
            StopAllCoroutines();

            // 只有当玩家仍然存在且当前控制器仍然有效时才重置颜色
            if (player != null && player.GameObject != null)
            {
                // 检查玩家是否仍然登录了称号
                var badgeManager = Plugin.plugin.BadgeManager;
                var currentBadge = badgeManager.GetPlayerBadge(player);

                if (currentBadge == null || currentBadge.BadgeType == BadgeType.Rainbow)
                {
                    // 如果玩家没有登录或者登录的是彩虹称号，才重置
                    player.RankColor = "default";
                }
                else
                {
                    // 如果玩家登录的是其他类型称号，重新应用正确的颜色
                    badgeManager.ApplyBadgeDirectly(player, currentBadge);
                }
            }
        }
    }
}