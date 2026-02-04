// BroadcastMessage.cs
using System;

namespace DreamPlugin.BroadcastSystem
{
    internal class BroadcastMessage
    {
        public string Text { get; }
        public DateTime ExpiryTime { get; }
        public DateTime CreationTime { get; }
        public float Duration { get; }

        public BroadcastMessage(string text, float duration)
        {
            Text = text;
            Duration = duration;
            CreationTime = DateTime.UtcNow;
            ExpiryTime = CreationTime.AddSeconds(duration);
        }

        public int RemainingSeconds => Math.Max(0, (int)Math.Ceiling((ExpiryTime - DateTime.UtcNow).TotalSeconds));
        public bool IsExpired => DateTime.UtcNow >= ExpiryTime;

        public bool IsWithinDedupWindow(float windowSeconds)
        {
            return (DateTime.UtcNow - CreationTime).TotalSeconds <= windowSeconds;
        }
    }
}