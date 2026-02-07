namespace RExiled.Events.EventArgs.Player
{
    public class DoorInteractingEventArgs : System.EventArgs
    {
        public RExiled.API.Features.Player Player { get; }
        public global::Door Door { get; }
        public bool IsAllowed { get; set; } = true;

        public DoorInteractingEventArgs(
            RExiled.API.Features.Player player,
            global::Door door)
        {
            Player = player;
            Door = door;
        }
    }
}