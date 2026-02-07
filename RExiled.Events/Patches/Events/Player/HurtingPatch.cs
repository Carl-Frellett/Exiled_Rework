using HarmonyLib;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;
using UnityEngine;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
    internal class HurtingPatch
    {
        internal static readonly HashSet<int> DeathProcessing = new HashSet<int>();

        public static bool Prefix(ref PlayerStats.HitInfo info, GameObject go)
        {
            if (go == null) return true;

            RExiled.API.Features.Player target = RExiled.API.Features.Player.Get(go);
            if (target == null) return true;

            RExiled.API.Features.Player attacker = null;
            if (!string.IsNullOrEmpty(info.Attacker))
            {
                if (int.TryParse(info.Attacker, out int attackerId))
                {
                    GameObject attackerGo = info.GetPlayerObject();
                    if (attackerGo != null)
                    {
                        attacker = RExiled.API.Features.Player.Get(attackerGo);
                    }
                }
                else if (info.Attacker != "ARTIFICIALDEGEN")
                {
                    GameObject attackerGo = info.GetPlayerObject();
                    if (attackerGo != null)
                    {
                        attacker = RExiled.API.Features.Player.Get(attackerGo);
                    }
                }
            }

            var hurtingEv = new HurtingEventArgs(attacker, target, ref info);
            RExiled.Events.Handlers.Player.OnHurting(hurtingEv);

            if (!hurtingEv.IsAllowed)
                return false;

            info.Amount = hurtingEv.Amount;

            if (info.Amount <= 0f)
                return true;

            return true;
        }

        public static void Postfix(GameObject go, PlayerStats.HitInfo info)
        {
            if (go == null) return;

            RExiled.API.Features.Player target = RExiled.API.Features.Player.Get(go);
            if (target == null) return;

            RExiled.API.Features.Player attacker = null;
            GameObject attackerGo = info.GetPlayerObject();
            if (attackerGo != null)
            {
                attacker = RExiled.API.Features.Player.Get(attackerGo);
            }

            var hurtEv = new HurtEventArgs(attacker, target, info.Amount, info.GetDamageType());
            RExiled.Events.Handlers.Player.OnHurt(hurtEv);

            if (target.IsDead)
            {
                DeathProcessing.Remove(target.Id);

                var diedEv = new DiedEventArgs(attacker, target, info.GetDamageType());
                RExiled.Events.Handlers.Player.OnDied(diedEv);
            }
            else if (DeathProcessing.Contains(target.Id))
            {
                DeathProcessing.Remove(target.Id);
            }
        }

        private static bool CheckIfDying(RExiled.API.Features.Player target, float damageAmount, PlayerStats.HitInfo hitInfo)
        {
            if (target.IsGodModeEnabled) return false;

            if (hitInfo.GetDamageType() == DamageTypes.Grenade && !target.ReferenceHub.characterClassManager.IsAnyScp())
                return true;

            float health = target.Health;
            float artificial = target.ReferenceHub.playerStats.unsyncedArtificialHealth;
            float ratio = target.ReferenceHub.playerStats.artificialNormalRatio;

            if (artificial > 0f && hitInfo.Attacker != "ARTIFICIALDEGEN")
            {
                float reduced = damageAmount * ratio;
                float remainder = damageAmount - reduced;
                float finalArtificial = artificial - reduced;
                if (finalArtificial < 0f)
                    remainder += Mathf.Abs(finalArtificial);
                return (health - remainder) < 1f;
            }

            return (health - damageAmount) < 1f;
        }
    }
}