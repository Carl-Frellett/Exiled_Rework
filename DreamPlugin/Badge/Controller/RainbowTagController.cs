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
            if (player != null && player.GameObject != null)
            {
                player.RankName = null;
                player.RankColor = string.Empty;
            }
        }
    }
}