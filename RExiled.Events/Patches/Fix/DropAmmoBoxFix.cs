using HarmonyLib;

namespace RExiled.Events.Patches.Fix
{
    [HarmonyPatch(typeof(AmmoBox), nameof(AmmoBox.CallCmdDrop))]
    public class DropAmmoBoxFix
    {
        public static bool Prefix() => false;
    }
}