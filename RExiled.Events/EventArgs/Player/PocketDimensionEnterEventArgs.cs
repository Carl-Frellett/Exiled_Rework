namespace RExiled.Events.EventArgs.Player
{
    public class PocketDimensionEnterEventArgs : System.EventArgs
    {
        public PocketDimensionEnterEventArgs(RExiled.API.Features.Player player)
        {
            Player = player;
            IsAllow = true;
        }

        public RExiled.API.Features.Player Player { get; }

        public bool IsAllow { get; set; }
    }
}