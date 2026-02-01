using HarmonyLib;
using RExiled.API.Enums;
using RExiled.API.Features;
using System;

namespace RExiled.Events
{
    public class Events : Plugin<Config>
    {
        private int patchesCounter;
        private static DateTime roundStartTime;

        public delegate void CustomEventHandler<TEventArgs>(TEventArgs ev) where TEventArgs : System.EventArgs;
        public delegate void CustomEventHandler();

        public override PluginPriority Priority { get; } = PluginPriority.First;
        public Harmony Harmony { get; private set; }

        public override string Author => "Carl Frellett & Exiled Team";

        public override void OnEnabled()
        {
            base.OnEnabled();

            RExiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundEnded += OnRoundEndedOrRestarted;
            RExiled.Events.Handlers.Server.RoundRestarted += OnRoundEndedOrRestarted;

            Patch();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundEnded -= OnRoundEndedOrRestarted;
            RExiled.Events.Handlers.Server.RoundRestarted -= OnRoundEndedOrRestarted;

            Unpatch();
        }

        private void OnRoundStarted()
        {
            roundStartTime = DateTime.Now;
        }

        private void OnRoundEndedOrRestarted()
        {
            roundStartTime = DateTime.MinValue;
        }

        public static float GetRoundDuration()
        {
            if (roundStartTime == DateTime.MinValue)
                return -1f;

            return (float)(DateTime.Now - roundStartTime).TotalSeconds;
        }

        public void Patch()
        {
            try
            {
                Harmony = new Harmony($"exiled.events.{++patchesCounter}");
                Harmony.PatchAll();
                Log.Debug("Events patched successfully!");
            }
            catch (Exception exception)
            {
                Log.Error($"Patching failed! {exception}");
            }
        }

        public void Unpatch()
        {
            Log.Debug("Unpatching events...");
            Harmony.UnpatchAll();
            Log.Debug("All events have been unpatched complete. Goodbye!");
        }
    }
}