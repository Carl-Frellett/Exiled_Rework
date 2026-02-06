using System;
using RExiled.API.Features;
using UnityEngine;

namespace RExiled.Events.EventArgs.Player
{
    public class ItemDroppedEventArgs : System.EventArgs
    {
        public ItemDroppedEventArgs(RExiled.API.Features.Player player, Pickup pickup, ItemType itemId, float durability)
        {
            Player = player;
            Pickup = pickup;
            ItemId = itemId;
            Durability = durability;
        }

        public RExiled.API.Features.Player Player { get; }

        public Pickup Pickup { get; }

        public ItemType ItemId { get; }

        public float Durability { get; }
    }
}