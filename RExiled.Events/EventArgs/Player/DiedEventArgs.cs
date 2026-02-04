namespace RExiled.Events.EventArgs.Player
{
    public class DiedEventArgs : System.EventArgs
    {
        public DiedEventArgs(RExiled.API.Features.Player killer, RExiled.API.Features.Player target, DamageTypes.DamageType damageType)
        {
            Killer = killer;
            Target = target;
            DamageType = damageType;
        }

        public RExiled.API.Features.Player Killer { get; }
        public RExiled.API.Features.Player Target { get; }
        public DamageTypes.DamageType DamageType { get; }
    }
}