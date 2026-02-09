using HarmonyLib;
using MEC;
using RExiled.API.Enums;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using RExiled.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RExiled.Events
{
    public class Events : Plugin<Config>
    {
        private int patchesCounter;
        private static DateTime roundStartTime;

        private CoroutineHandle _idleCheckCoroutine;
        private static DateTime LastActiveTime;
        private static bool WasLastCheckIdle;
        private static bool IdleSent;

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
            RExiled.Events.Handlers.Player.Joined += OnPlayerJoined;
            RExiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;

            Patch();
            StartIdler();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundEnded -= OnRoundEndedOrRestarted;
            RExiled.Events.Handlers.Server.RoundRestarted -= OnRoundEndedOrRestarted;
            RExiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
            RExiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;

            StopIdler();
            Unpatch();
        }
        public void OnWaitingForPlayers()
        {
            RExiled.API.Features.Player.IdsCache.Clear();
            RExiled.API.Features.Player.UserIdsCache.Clear();
            RExiled.API.Features.Player.Dictionary.Clear();

            ConfigManager.Reload();

            RoundSummary.RoundLock = false;
        }
        private void OnRoundStarted() => roundStartTime = DateTime.Now;
        private void OnRoundEndedOrRestarted() => roundStartTime = DateTime.MinValue;
        public static float GetRoundDuration()
        {
            if (roundStartTime == DateTime.MinValue) return -1f;
            return (float)(DateTime.Now - roundStartTime).TotalSeconds;
        }

        private void OnPlayerJoined(JoinedEventArgs ev)
        {
            MarkAsActive();
        }

        private void StartIdler()
        {
            LastActiveTime = DateTime.UtcNow;
            WasLastCheckIdle = false;
            IdleSent = false;
            _idleCheckCoroutine = Timing.RunCoroutine(IdlerCheckRoutine(), Segment.Update);
        }

        private void StopIdler()
        {
            if (_idleCheckCoroutine.IsRunning)
                Timing.KillCoroutines(_idleCheckCoroutine);

            Time.timeScale = 1f;
            Application.targetFrameRate = -1;
        }

        private void MarkAsActive()
        {
            if (WasLastCheckIdle)
            {
                Log.SendRaw("Server activity detected. Resuming normal operation.");
            }
            WasLastCheckIdle = false;
            IdleSent = false;
            LastActiveTime = DateTime.UtcNow;
            Time.timeScale = 1f;
            Application.targetFrameRate = 60;
        }

        private IEnumerator<float> IdlerCheckRoutine()
        {
            const float checkInterval = 5f;
            const float idleThresholdMinutes = 3f;

            while (true)
            {
                yield return Timing.WaitForSeconds(checkInterval);

                bool isIdle = Player.List.Count() == 0;

                if (isIdle && !WasLastCheckIdle)
                {
                    LastActiveTime = DateTime.UtcNow;
                    Log.SendRaw("Server is now idle.");
                }

                if (isIdle && WasLastCheckIdle)
                {
                    if ((DateTime.UtcNow - LastActiveTime).TotalMinutes >= idleThresholdMinutes)
                    {
                        if (!IdleSent)
                        {
                            Log.SendRaw($"Server has been idle for {idleThresholdMinutes} minutes. Entering low-power mode!");
                            IdleSent = true;
                        }
                        Time.timeScale = 0.01f;
                        Application.targetFrameRate = 1;
                    }
                }

                if (!isIdle)
                {
                    MarkAsActive();
                }

                WasLastCheckIdle = isIdle;
            }
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