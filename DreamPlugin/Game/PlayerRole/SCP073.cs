using DreamPlugin.Game.CustomRole.Extensions;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;

namespace DreamPlugin.Game.CustomRole
{
    public class Scp073Role : CustomRole
    {
        public Scp073Role()
        {
            Name = "SCP-073";
            SpawnDescription = "你是<color=blue>SCP-073</color> <i>该隐</i> <i>对伤害有抗性, 对攻击有增强</i>";
            DiedDescription = "";
            SpawnCondition = SpawnConditionType.TeamSpawn;
            SpawnRoleType = RoleType.NtfCadet;
            SpawnCapacityLimit = 5;
            SpawnRoleCapacityLimit = 1;
            RoleHealth = 200;
            RoleMaxHealth = 200;
            RoleInventory = new List<ItemType>
            {
                ItemType.GunLogicer, ItemType.KeycardO5, ItemType.SCP500,
                ItemType.Medkit, ItemType.Adrenaline, ItemType.WeaponManagerTablet, ItemType.Radio
            };
            IsJoinSpawnQueue = true;
        }

        public override void OnSpawn()
        {
            CurrentPlayer.AdrenalineHealth += 50;
            RExiled.Events.Handlers.Player.Hurting += OnHurting;
        }

        public override void OnDestroy()
        {
            RExiled.Events.Handlers.Player.Hurting -= OnHurting;
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == CurrentPlayer)
                ev.Amount *= 1.3f;
            if (ev.Target == CurrentPlayer)
                ev.Amount *= 0.4f;
        }
    }
}