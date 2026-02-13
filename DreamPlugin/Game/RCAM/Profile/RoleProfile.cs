using System.Collections.Generic;
using UnityEngine;

namespace DreamPlugin.Game.RCAM.Profile
{
    public class RoleProfile
    {
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public Vector3? SpawnPosition { get; set; } = null;
        public List<ItemType> StartingItems { get; set; } = new List<ItemType>();
    }
}