namespace RExiled.Events.EventArgs.Player
{
    public class WarheadPanelInteractingEventArgs : System.EventArgs
    {
        public RExiled.API.Features.Player Player { get; }
        public bool IsAllowed { get; set; } = true;
        public string RequiredPermission { get; set; } = "CONT_LVL_3";

        public WarheadPanelInteractingEventArgs(RExiled.API.Features.Player player)
        {
            Player = player;
        }
    }
}