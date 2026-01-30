using System;

using GameCore;

namespace RExiled.API.Features
{
    public static class Round
    {
        public static TimeSpan ElapsedTime => RoundStart.RoundLenght;

        public static DateTime StartedTime => DateTime.Now - ElapsedTime;

        public static bool IsStarted => RoundSummary.RoundInProgress();

        public static bool IsLocked
        {
            get => RoundSummary.RoundLock;
            set => RoundSummary.RoundLock = value;
        }

        public static bool IsLobbyLocked
        {
            get => RoundStart.LobbyLock;
            set => RoundStart.LobbyLock = value;
        }

        public static void Restart() => Server.Host.ReferenceHub.playerStats.Roundrestart();


        public static void Start() => CharacterClassManager.ForceRoundStart();
    }
}
