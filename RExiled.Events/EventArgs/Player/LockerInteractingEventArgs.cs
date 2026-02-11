namespace RExiled.Events.EventArgs.Player
{
    using System;
    using RExiled.API.Features;

    public class LockerInteractingEventArgs : EventArgs
    {
        public LockerInteractingEventArgs(
            Player player,
            Locker locker,
            LockerChamber chamber,
            int lockerId,
            int chamberNumber,
            bool hasPermission)
        {
            Player = player;
            Locker = locker;
            Chamber = chamber;
            LockerId = lockerId;
            ChamberNumber = chamberNumber;
            HasPermission = hasPermission;
            IsAllowed = hasPermission;
        }

        public Player Player { get; }
        public Locker Locker { get; }
        public LockerChamber Chamber { get; }
        public int LockerId { get; }
        public int ChamberNumber { get; }
        public bool HasPermission { get; }
        public bool IsAllowed { get; set; }
    }
}