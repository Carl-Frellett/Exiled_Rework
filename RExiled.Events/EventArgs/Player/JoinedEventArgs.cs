using System;

using RExiled.API.Features;

namespace RExiled.Events.EventArgs
{
    public class JoinedEventArgs : System.EventArgs
    {
        public JoinedEventArgs(RExiled.API.Features.Player player) => Player = player;

        public RExiled.API.Features.Player Player { get; }
    }
}
