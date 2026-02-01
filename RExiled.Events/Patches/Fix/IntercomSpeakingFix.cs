using HarmonyLib;
using RExiled.API.Features;
using System;

namespace RExiled.Events.Patches.Fix
{
    [HarmonyPatch(typeof(Intercom), nameof(Intercom.CallCmdSetTransmit))]
    public class IntercomSpeakingEvent
    {
        public static bool Prefix(Intercom __instance, bool player)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;

                if (player)
                {
                    if (!__instance.ServerAllowToSpeak())
                        return false;

                    Intercom.host.RequestTransmission(__instance.gameObject);
                }
                else
                {
                    if (Intercom.host.Networkspeaker != __instance.gameObject)
                        return false;

                    Intercom.host.RequestTransmission(null);
                }

                return false;
            }
            catch (Exception exception)
            {
                Log.Error($"IntercomSpeakingFix error: {exception}");
                return true;
            }
        }
    }
}