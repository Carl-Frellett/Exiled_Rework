using DreamPlugin.Game;
using RExiled.API.Features;

namespace DreamPlugin
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "DreamPlugin";
        public override string Author => "Carl Frellett";

        private CommandHandler CommandHandler = new CommandHandler();
        private EventHandler EventHandler = new EventHandler();

        public override void OnEnabled()
        {
            base.OnEnabled();
            CommandHandler.RegisterEvents();
            EventHandler.RegisterEvents();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            CommandHandler.UnregisterEvents();
            EventHandler.UnregisterEvents();
        }
    }
}
