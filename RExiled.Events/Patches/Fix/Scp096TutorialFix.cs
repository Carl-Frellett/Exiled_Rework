using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RExiled.Events.Patches.Fix
{
    [HarmonyPatch(typeof(Scp096PlayerScript), nameof(Scp096PlayerScript.ProcessLooking))]
    public class Scp096TutorialFix
    {
        public static void Postfix(Scp096PlayerScript __instance)
        {
            if (__instance._processLookingQueue != null &&
                __instance._processLookingQueue.Count > 0)
            {
                var filtered = new Queue<GameObject>(
                    __instance._processLookingQueue.Where(go =>
                    {
                        var hub = ReferenceHub.GetHub(go);
                        return hub?.characterClassManager?.CurClass != RoleType.Tutorial;
                    })
                );

                __instance._processLookingQueue = filtered;
            }
        }
    }
}