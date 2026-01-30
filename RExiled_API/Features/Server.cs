using System.Reflection;

using Mirror;

namespace RExiled.API.Features
{
    public static class Server
    {
        private static Player host;
        private static global::Broadcast broadcast;
        private static BanPlayer banPlayer;
        private static MethodInfo sendSpawnMessage;

        public static Player Host
        {
            get
            {
                if (host == null || host.ReferenceHub == null)
                    host = new Player(PlayerManager.localPlayer);

                return host;
            }
        }

        public static global::Broadcast Broadcast
        {
            get
            {
                if (broadcast == null)
                    broadcast = PlayerManager.localPlayer.GetComponent<global::Broadcast>();

                return broadcast;
            }
        }

        public static BanPlayer BanPlayer
        {
            get
            {
                if (banPlayer == null)
                    banPlayer = PlayerManager.localPlayer.GetComponent<BanPlayer>();

                return banPlayer;
            }
        }

        public static MethodInfo SendSpawnMessage
        {
            get
            {
                if (sendSpawnMessage == null)
                {
                    sendSpawnMessage = typeof(NetworkServer).GetMethod(
                        "SendSpawnMessage",
                        BindingFlags.NonPublic | BindingFlags.Static);
                }

                return sendSpawnMessage;
            }
        }

        public static string Name
        {
            get => ServerConsole._serverName;
            set
            {
                ServerConsole._serverName = value;
                ServerConsole.singleton.RefreshServerName();
            }
        }

        public static ushort Port
        {
            get => ServerStatic.ServerPort;
            set => ServerStatic.ServerPort = value;
        }

        public static bool FriendlyFire
        {
            get => ServerConsole.FriendlyFire;
            set => ServerConsole.FriendlyFire = value;
        }
    }
}
