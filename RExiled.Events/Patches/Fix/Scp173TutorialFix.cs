using HarmonyLib;
using UnityEngine;

namespace RExiled.Events.Patches.Fix
{
    [HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.LookFor173))]
    public class Scp173TutorialFix
    {
        public static bool Prefix(
            Scp173PlayerScript __instance,
            GameObject scp,
            bool angleCheck,
            ref bool __result)
        {
            var ccm = __instance.GetComponent<CharacterClassManager>();
            if (ccm != null && ccm.CurClass == RoleType.Tutorial)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}