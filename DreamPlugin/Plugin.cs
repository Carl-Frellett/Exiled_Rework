using DreamPlugin.Badge;
using DreamPlugin.Game;
using DreamPlugin.Game.PlayerRole;
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
        private InfiniteAmmo InfiniteAmmo = new InfiniteAmmo();
        private BigGuard BigGuard = new BigGuard();
        private InventoryAccess InventoryAccess = new InventoryAccess();
        public SCP073 SCP073 = new SCP073();
        public SCP550 SCP550 = new SCP550();
        public SCP6000 SCP6000 = new SCP6000();

        public BadgeManager BadgeManager;
        private WebServer _webServer;

        public override void OnEnabled()
        {
            base.OnEnabled();
            plugin = this;
            CommandHandler.RegisterEvents();
            EventHandler.RegisterEvents();
            InfiniteAmmo.RegisterEvents();
            BigGuard.RegisterEvents();
            SCP073.RegisterEvents();
            SCP550.RegisterEvents();
            SCP6000.RegisterEvents();
            InventoryAccess.RegisterEvents();

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
            InfiniteAmmo.UnregisterEvents();
            BigGuard.UnregisterEvents();
            SCP073.UnregisterEvents();
            SCP550.UnregisterEvents();
            SCP6000.UnregisterEvents();
            InventoryAccess.UnregisterEvents();

            BadgeManager.SaveBadges();

            _webServer?.Dispose();
            _webServer = null;
        }
    }
}
