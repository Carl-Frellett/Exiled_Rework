using System;
using RExiled.API.Features;

namespace RExiled.Events.EventArgs.Player
{
    public class HurtingEventArgs : System.EventArgs
    {
        public HurtingEventArgs(RExiled.API.Features.Player attacker, RExiled.API.Features.Player target, ref PlayerStats.HitInfo hitInfo)
        {
            Attacker = attacker;
            Target = target;
            HitInfo = hitInfo;
        }

        public RExiled.API.Features.Player Attacker { get; }
        public RExiled.API.Features.Player Target { get; }
        public ref PlayerStats.HitInfo HitInfo => ref hitInfo;

        private PlayerStats.HitInfo hitInfo;
    }
}