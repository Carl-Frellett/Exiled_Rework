namespace RExiled.Events.Patches.Events.Player
{
    using CustomPlayerEffects;
    using HarmonyLib;
    using RExiled.Events.EventArgs.Player;
    using UnityEngine;

    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMovePlayer))]
    internal static class PocketDimensionEnterPatch
    {
        private static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true))
                    return false;

                if (ply == null)
                    return false;

                var characterClassManager = ply.GetComponent<CharacterClassManager>();
                if (characterClassManager == null)
                    return false;

                if (!ServerTime.CheckSynchronization(t) ||
                    !__instance.iAm106 ||
                    Vector3.Distance(__instance.GetComponent<PlyMovementSync>().RealModelPosition, ply.transform.position) >= 3f ||
                    !characterClassManager.IsHuman())
                    return false;

                if (characterClassManager.GodMode)
                    return false;

                if (characterClassManager.Classes.SafeGet(characterClassManager.CurClass).team == Team.SCP)
                    return false;

                var player = RExiled.API.Features.Player.Get(ply);
                if (player == null)
                    return false;

                var enteringEventArgs = new EnteringPocketDimensionEventArgs(player, isAllowed: true);
                RExiled.Events.Handlers.Player.OnEnteringPocketDimension(enteringEventArgs);

                if (!enteringEventArgs.IsAllowed)
                    return false;

                ply.GetComponent<PlyMovementSync>().OverridePosition(Vector3.down * 1998.5f, 0f, true);

                var effectsController = ply.GetComponentInParent<PlayerEffectsController>();
                if (effectsController != null)
                {
                    var corroding = effectsController.GetEffect<Corroding>("Corroding");
                    if (corroding != null)
                    {
                        corroding.isInPd = true;
                        effectsController.EnableEffect("Corroding");
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                RExiled.API.Features.Log.Error($"[RExiled] PocketDimensionEnterPatch error: {ex}");
                return true;
            }
        }
    }
}