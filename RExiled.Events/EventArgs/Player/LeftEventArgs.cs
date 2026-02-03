using RExiled.API.Features;

namespace RExiled.Events.EventArgs.Player
{
    public class LeftEventArgs : JoinedEventArgs
    {
        public LeftEventArgs(RExiled.API.Features.Player player)
            : base(player)
        {
        }
    }
}