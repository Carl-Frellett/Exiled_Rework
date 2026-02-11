namespace RExiled.Events.EventArgs.Player
{
    using System;
    public class EnteringPocketDimensionEventArgs : EventArgs
    {
        public EnteringPocketDimensionEventArgs(RExiled.API.Features.Player player, bool isAllowed = true)
        {
            Player = player;
            IsAllowed = isAllowed;
        }

        public RExiled.API.Features.Player Player { get; }

        public bool IsAllowed { get; set; }
    }
}