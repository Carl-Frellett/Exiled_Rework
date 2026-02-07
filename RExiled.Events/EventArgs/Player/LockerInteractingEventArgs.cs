namespace RExiled.Events.EventArgs.Player
{
    public class LockerInteractingEventArgs : System.EventArgs
    {
        public RExiled.API.Features.Player Player { get; }
        public global::Locker Locker { get; }
        public global::LockerChamber Chamber { get; }
        public int LockerId { get; }
        public int ChamberNumber { get; }
        public bool IsAllowed { get; set; } = true;

        public LockerInteractingEventArgs(
            RExiled.API.Features.Player player,
            global::Locker locker,
            global::LockerChamber chamber,
            int lockerId,
            int chamberNumber)
        {
            Player = player;
            Locker = locker;
            Chamber = chamber;
            LockerId = lockerId;
            ChamberNumber = chamberNumber;
        }
    }
}