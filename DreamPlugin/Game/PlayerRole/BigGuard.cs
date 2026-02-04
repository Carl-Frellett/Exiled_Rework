using MEC;
using RExiled.API.Features;
using System.Collections.Generic;
using System.Linq;

namespace DreamPlugin.Game.PlayerRole
{
    public class BigGuard
    {
        public void RegisterEvents()
        { 
            RExiled.Events.Handlers.Server.RoundStarted += OnRoundStartSpawnBigGuard;
        }

        public void UnregisterEvents()
        {
            RExiled.Events.Handlers.Server.RoundStarted -= OnRoundStartSpawnBigGuard;
        }

        public void OnRoundStartSpawnBigGuard()
        {
            Timing.CallDelayed(1.5f, () =>
            {
                if (Player.List.Count() >= 5)
                {
                    var guards = Player.List.Where(p => p.Role == RoleType.FacilityGuard).ToList();

                    if (guards.Count > 0)
                    {
                        var randomGuard = guards[UnityEngine.Random.Range(0, guards.Count)];

                        BroadcastSystem.BroadcastSystem.ShowToPlayer(randomGuard, "[个人消息] 你是<color=blue>保安大队长</size>!", 6);

                        List<ItemType> BigGuardItems = new List<ItemType>()
                    {
                        ItemType.GunProject90,
                        ItemType.GunUSP,
                        ItemType.WeaponManagerTablet,
                        ItemType.Radio,
                        ItemType.SCP500,
                        ItemType.KeycardSeniorGuard,
                    };

                        randomGuard.ResetInventory(BigGuardItems);
                        randomGuard.Health = 120;
                        randomGuard.MaxHealth = 120;
                    }
                }
            });
        }
    }
}
