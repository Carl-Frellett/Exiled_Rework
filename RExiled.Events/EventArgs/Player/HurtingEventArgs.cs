namespace RExiled.Events.EventArgs.Player
{
    public class HurtingEventArgs : System.EventArgs
    {
        public HurtingEventArgs(RExiled.API.Features.Player attacker, RExiled.API.Features.Player target, ref PlayerStats.HitInfo hitInfo)
        {
            Attacker = attacker;
            Target = target;
            Amount = hitInfo.Amount;
            DamageType = hitInfo.GetDamageType();
            IsAllowed = true;
        }

        public RExiled.API.Features.Player Attacker { get; }
        public RExiled.API.Features.Player Target { get; }
        public float Amount { get; set; }
        public DamageTypes.DamageType DamageType { get; }
        public bool IsAllowed { get; set; }
    }
}