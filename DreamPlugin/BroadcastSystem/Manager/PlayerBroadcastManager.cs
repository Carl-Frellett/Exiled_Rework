using System.Collections.Generic;
using System.Text;
using RExiled.API.Features;

namespace DreamPlugin.BroadcastSystem.Manager
{
    internal class PlayerBroadcastManager
    {
        private readonly Player player;
        private readonly List<BroadcastMessage> messages = new List<BroadcastMessage>();
        private const float DeduplicationWindow = 1f;

        public PlayerBroadcastManager(Player player)
        {
            this.player = player;
        }

        public void AddMessage(string text, float duration)
        {
            if (string.IsNullOrEmpty(text)) return;

            var existing = messages.Find(msg =>
                msg.Text == text &&
                msg.IsWithinDedupWindow(DeduplicationWindow));

            if (existing != null)
            {
                messages.Remove(existing);
            }

            messages.Add(new BroadcastMessage(text, duration));
        }

        public string BuildText()
        {
            if (messages.Count == 0) return null;

            var sb = new StringBuilder("<size=30>");
            bool hasValid = false;

            foreach (var msg in messages.ToArray())
            {
                if (msg.IsExpired)
                {
                    messages.Remove(msg);
                    continue;
                }

                sb.Append($"<size=15>[{msg.RemainingSeconds}]</size> {msg.Text}\n");
                hasValid = true;
            }

            if (!hasValid)
            {
                messages.Clear();
                return null;
            }

            if (sb.Length > 11)
                sb.Length -= 1;

            sb.Append("</size>");
            return sb.ToString();
        }
        public List<BroadcastMessage> GetActiveMessages()
        {
            var active = new List<BroadcastMessage>();
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                if (messages[i].IsExpired)
                    messages.RemoveAt(i);
                else
                    active.Add(messages[i]);
            }
            return active;
        }
        public bool HasMessages => messages.Count > 0;
        public void Clear() => messages.Clear();
    }
}