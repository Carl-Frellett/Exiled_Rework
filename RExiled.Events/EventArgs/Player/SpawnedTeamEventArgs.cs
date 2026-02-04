using System.Collections.Generic;

namespace RExiled.Events.EventArgs.Player
{
    public class SpawnedTeamEventArgs : System.EventArgs
    {
        public SpawnedTeamEventArgs(bool isChaos, List<RExiled.API.Features.Player> players)
        {
            IsChaos = isChaos;
            Players = players;
        }

        public bool IsChaos { get; }
        public List<RExiled.API.Features.Player> Players { get; }
    }
}