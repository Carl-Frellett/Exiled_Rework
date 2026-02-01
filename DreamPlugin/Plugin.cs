using DreamPlugin.Badge;
using DreamPlugin.Game;
using RExiled.API.Features;

namespace DreamPlugin
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "DreamPlugin";
        public override string Author => "Carl Frellett";

        public static Plugin plugin { get; private set; }

        private CommandHandler CommandHandler = new CommandHandler();
        private EventHandler EventHandler = new EventHandler();

        public BadgeManager BadgeManager;

        private WebServer _webServer;
        public override void OnEnabled()
        {
            base.OnEnabled();
            plugin = this;
            CommandHandler.RegisterEvents();
            EventHandler.RegisterEvents();

            BadgeManager = new BadgeManager();
            BadgeManager.LoadBadges();

            _webServer = new WebServer();
            _webServer.Start();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            CommandHandler.UnregisterEvents();
            EventHandler.UnregisterEvents();

            BadgeManager.SaveBadges();

            _webServer?.Dispose();
            _webServer = null;
        }
    }
}
