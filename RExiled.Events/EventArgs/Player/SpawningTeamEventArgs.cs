using System.Collections.Generic;

namespace RExiled.Events.EventArgs.Player
{
    public class SpawningTeamEventArgs : System.EventArgs
    {
        public SpawningTeamEventArgs(bool isChaos, int maxRespawnAmount, List<RExiled.API.Features.Player> players)
        {
            IsChaos = isChaos;
            MaxRespawnAmount = maxRespawnAmount;
            Players = players;
            IsAllowed = true;
        }

        public bool IsChaos { get; set; }

        public int MaxRespawnAmount { get; set; }

        public List<RExiled.API.Features.Player> Players { get; set; }

        public bool IsAllowed { get; set; }
    }
}