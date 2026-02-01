using RExiled.API.Features;
using System.Collections;
using System.Text;
using UnityEngine;

namespace DreamPlugin.Badge.Controller
{
    public class DynamicBadgeController : MonoBehaviour
    {
        private Player player;
        private string fullContent;
        private string color;
        private BadgeType badgeType;
        private string[] rainbowColors;
        private float textInterval;
        private float colorInterval;

        private StringBuilder currentContent;
        private int currentLength;
        private float textTimer;
        private float colorTimer;
        private int rainbowIndex;
        private bool isShowingUnderscore = true;
        private float underscoreTimer = 0f;
        private bool isCompleteCycle = false;
        private float completeTimer = 0f;
        private float underscoreInterval = 0.2f;
        private float completeDisplayTime = 2f;

        public void Initialize(Player targetPlayer, string content, string badgeColor,
            BadgeType type, string[] colorArray)
        {
            player = targetPlayer;
            fullContent = content;
            color = badgeColor;
            badgeType = type;
            rainbowColors = colorArray;

            textInterval = Plugin.plugin.Config.TextChangeInterval;
            colorInterval = Plugin.plugin.Config.ColorChangeInterval;

            currentContent = new StringBuilder();
            currentLength = 0;
            rainbowIndex = 0;
            isShowingUnderscore = true;
            isCompleteCycle = false;

            player.RankName = "_";

            StartCoroutine(DynamicBadgeRoutine());
        }

        private IEnumerator DynamicBadgeRoutine()
        {
            while (player != null)
            {
                if (!isCompleteCycle)
                {
                    if (textTimer >= textInterval)
                    {
                        UpdateContent();
                        textTimer = 0f;
                    }
                    textTimer += Time.deltaTime;

                    if (currentLength > 0)
                    {
                        if (underscoreTimer >= underscoreInterval)
                        {
                            isShowingUnderscore = !isShowingUnderscore;
                            UpdateDisplayText();
                            underscoreTimer = 0f;
                        }
                        underscoreTimer += Time.deltaTime;
                    }
                }
                else
                {
                    if (completeTimer >= completeDisplayTime)
                    {
                        currentContent.Clear();
                        currentLength = 0;
                        isShowingUnderscore = true;
                        isCompleteCycle = false;
                        player.RankName = "_";
                        completeTimer = 0f;
                    }
                    else
                    {
                        completeTimer += Time.deltaTime;
                    }
                }

                if (colorTimer >= colorInterval)
                {
                    UpdateColor();
                    colorTimer = 0f;
                }
                colorTimer += Time.deltaTime;

                yield return null;
            }

            Destroy(this);
        }

        private void UpdateContent()
        {
            if (currentLength < fullContent.Length)
            {
                currentContent.Append(fullContent[currentLength]);
                currentLength++;
                isShowingUnderscore = true;
                UpdateDisplayText();
            }
            else
            {
                isCompleteCycle = true;
                player.RankName = fullContent;
            }
        }

        private void UpdateDisplayText()
        {
            if (isShowingUnderscore)
            {
                player.RankName = currentContent.ToString() + "_";
            }
            else
            {
                player.RankName = currentContent.ToString();
            }
        }

        private void UpdateColor()
        {
            switch (badgeType)
            {
                case BadgeType.SimpleDynamic:
                    player.RankColor = color;
                    break;
                case BadgeType.RainbowDynamic:
                    if (rainbowColors.Length > 0)
                    {
                        player.RankColor = rainbowColors[rainbowIndex];
                        rainbowIndex = (rainbowIndex + 1) % rainbowColors.Length;
                    }
                    break;
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();

            if (player != null && player.GameObject != null)
            {
                var badgeManager = Plugin.plugin.BadgeManager;
                var currentBadge = badgeManager.GetPlayerBadge(player);

                if (currentBadge == null || currentBadge.BadgeType == BadgeType.SimpleDynamic || currentBadge.BadgeType == BadgeType.RainbowDynamic)
                {
                    player.RankName = null;
                    player.RankColor = "default";
                }
                else
                {
                    badgeManager.ApplyBadgeDirectly(player, currentBadge);
                }
            }
        }
    }
}