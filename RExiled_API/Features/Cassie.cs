using MEC;

namespace RExiled.API.Features
{
    public static class Cassie
    {
        private static MTFRespawn _mtfRespawn;
        public static MTFRespawn mtfRespawn
        {
            get
            {
                if (_mtfRespawn == null)
                {
                    _mtfRespawn = PlayerManager.localPlayer.GetComponent<MTFRespawn>();
                }
                return _mtfRespawn;
            }
        }

        public static void CassieMessage(string msg, bool makeHold, bool makeNoise) => mtfRespawn.RpcPlayCustomAnnouncement(msg, makeHold, makeNoise);

        public static void DelayedCassieMessage(string msg, bool makeHold, bool makeNoise, float delay)
        {
            Timing.CallDelayed(delay, () => mtfRespawn.RpcPlayCustomAnnouncement(msg, makeHold, makeNoise));
        }
    }
}