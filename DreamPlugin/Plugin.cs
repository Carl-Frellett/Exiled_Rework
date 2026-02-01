using DreamPlugin.Chat;
using DreamPlugin.Game;
using RExiled.API.Features;

namespace DreamPlugin
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "DreamPlugin";
        public override string Author => "Carl Frellett";

        private ChatEventCommand ChatEventCommand = new ChatEventCommand();
        private EventHandler EventHandler = new EventHandler();

        public override void OnEnabled()
        {
            base.OnEnabled();
            ChatEventCommand.RegisterEvents();
            EventHandler.RegisterEvents();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            ChatEventCommand.UnregisterEvents();
            EventHandler.UnregisterEvents();
        }
    }
}
