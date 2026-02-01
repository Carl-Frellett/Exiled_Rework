using HarmonyLib;
using UnityEngine;

namespace RExiled.Events.Patches.Fix
{
	[HarmonyPatch(typeof(PlyMovementSync), nameof(PlyMovementSync.AntiFly))]
	public class AntiFlyPatch
	{ 
		public static bool Prefix (PlyMovementSync __instance, Vector3 pos) => false;
	}
}