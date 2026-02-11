using HarmonyLib;

namespace EXILED
{
	[HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
	public class ServerNamePatch
	{
		public static void Postfix()
        {
            ServerConsole._serverName += $"<color=#00000000><size=1>RExiled2</size></color>";
        }
	}
}