using System;
using RExiled.API.Features;

namespace RExiled.Events.EventArgs.Player
{
    public class DroppingItemEventArgs : System.EventArgs
    {
        public DroppingItemEventArgs(RExiled.API.Features.Player player, ItemType itemId, float durability, int inventoryIndex)
        {
            Player = player;
            ItemId = itemId;
            Durability = durability;
            InventoryIndex = inventoryIndex;
            IsAllowed = true;
        }

        public RExiled.API.Features.Player Player { get; }

        public ItemType ItemId { get; }

        public float Durability { get; }

        public int InventoryIndex { get; }

        public bool IsAllowed { get; set; }
    }
}