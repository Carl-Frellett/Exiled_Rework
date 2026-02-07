namespace RExiled.Events.EventArgs.Player
{
    public class GeneratorDoorOpeningEventArgs : System.EventArgs
    {
        public RExiled.API.Features.Player Player { get; }
        public global::Generator079 Generator { get; }
        public bool IsAllowed { get; set; } = true;

        public GeneratorDoorOpeningEventArgs(
            RExiled.API.Features.Player player,
            global::Generator079 generator)
        {
            Player = player;
            Generator = generator;
        }
    }
}