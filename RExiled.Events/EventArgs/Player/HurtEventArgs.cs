namespace RExiled.Events.EventArgs.Player
{
    public class HurtEventArgs : System.EventArgs
    {
        public HurtEventArgs(RExiled.API.Features.Player attacker, RExiled.API.Features.Player target, float amount, DamageTypes.DamageType damageType)
        {
            Attacker = attacker;
            Target = target;
            Amount = amount;
            DamageType = damageType;
        }

        public RExiled.API.Features.Player Attacker { get; }
        public RExiled.API.Features.Player Target { get; }
        public float Amount { get; }
        public DamageTypes.DamageType DamageType { get; }
    }
}