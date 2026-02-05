using System;
using RExiled.API.Features;
using UnityEngine;

namespace RExiled.Events.EventArgs.Player
{
    public class PickingUpItemEventArgs : System.EventArgs
    {
        public PickingUpItemEventArgs(RExiled.API.Features.Player player, Pickup pickup)
        {
            Player = player;
            Pickup = pickup;
            IsAllowed = true;
        }

        public RExiled.API.Features.Player Player { get; }

        public Pickup Pickup { get; set; }

        public bool IsAllowed { get; set; }
    }
}