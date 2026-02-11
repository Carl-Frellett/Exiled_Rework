using DreamPlugin.Game.CustomRole.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace DreamPlugin.Game.CustomRole
{
    public class FatBRole : CustomRole
    {
        public FatBRole()
        {
            Name = "胖逼";
            SpawnDescription = "你是<color=yellow>胖逼</color> <i>肥 鲜 嫩 润</i>";
            DiedDescription = "";
            SpawnCondition = SpawnConditionType.RoundStart;
            SpawnRoleType = RoleType.ClassD;
            SpawnCapacityLimit = 5;
            SpawnRoleCapacityLimit = 1;
            RoleHealth = 250;
            RoleMaxHealth = 250;
            RoleInventory = new List<ItemType>();
            IsJoinSpawnQueue = true;
        }

        public override void OnSpawn()
        {
            Vector3 scale = new Vector3(1.17f, 0.8f, 1.17f);
            CurrentPlayer.SetScale(scale.x, scale.y, scale.z);
        }

        public override void OnDestroy()
        {
        }
    }
}