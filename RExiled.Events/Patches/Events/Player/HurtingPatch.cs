using HarmonyLib;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using UnityEngine;

namespace RExiled.Events.Patches.Events.Player
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
    internal static class Hurting
    {
        public static void Prefix(PlayerStats __instance, ref PlayerStats.HitInfo info, GameObject go)
        {
            try
            {
                if (go == null)
                    return;

                var target = RExiled.API.Features.Player.Get(go);
                if (target == null)
                    return;

                RExiled.API.Features.Player attacker = null;
                GameObject attackerGo = info.GetPlayerObject();

                if (attackerGo != null)
                {
                    var attackerHub = ReferenceHub.GetHub(attackerGo);
                    if (attackerHub != null)
                        attacker = RExiled.API.Features.Player.Get(attackerHub);
                }

                var hurtingEv = new HurtingEventArgs(attacker, target, ref info);
                Handlers.Player.OnHurting(hurtingEv);

                if (hurtingEv.HitInfo.Amount <= 0f)
                    return;

                bool isDying = !target.IsGodModeEnabled && (target.Health - hurtingEv.HitInfo.Amount) < 1f;
                if (hurtingEv.HitInfo.Amount < 0f && !target.IsGodModeEnabled)
                {
                    isDying = true;
                }
                if (isDying)
                {
                    var dyingEv = new DyingEventArgs(attacker, target, ref info);
                    Handlers.Player.OnDying(dyingEv);

                    if (dyingEv.HitInfo.Amount <= 0f)
                        return;
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[RExiled] Hurting patch error: {ex}");
            }
        }
    }
}