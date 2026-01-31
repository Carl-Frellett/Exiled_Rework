using HarmonyLib;
using RExiled.API.Enums;
using RExiled.API.Features;
using System;

namespace RExiled.Events
{
    public sealed class Events : Plugin<Config>
    {
        private int patchesCounter;
        private Events()
        {
        }

        public delegate void CustomEventHandler<TEventArgs>(TEventArgs ev)
            where TEventArgs : System.EventArgs;

        public delegate void CustomEventHandler();
        public override PluginPriority Priority { get; } = PluginPriority.First;
        public Harmony Harmony { get; private set; }

        public override void OnEnabled()
        {
            base.OnEnabled();

            Patch();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            Unpatch();
        }

        public void Patch()
        {
            try
            {
                Harmony = new Harmony($"exiled.events.{++patchesCounter}");
#if DEBUG
                var lastDebugStatus = Harmony.DEBUG;
                Harmony.DEBUG = true;
#endif
                Harmony.PatchAll();
#if DEBUG
                Harmony.DEBUG = lastDebugStatus;
#endif
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
