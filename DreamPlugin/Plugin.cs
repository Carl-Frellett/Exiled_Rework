using DreamPlugin.Badge;
using DreamPlugin.Game;
using DreamPlugin.Game.RCAM;
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
        private InventoryAccess InventoryAccess = new InventoryAccess();
        private RoundCharacterAssignmentManager RoundCharacterAssignmentManager = new RoundCharacterAssignmentManager();

        public BadgeManager BadgeManager;
        private WebServer _webServer;

        public override void OnEnabled()
        {
            base.OnEnabled();
            plugin = this;
            CommandHandler.RegisterEvents();
            EventHandler.RegisterEvents();
            InfiniteAmmo.RegisterEvents();
            InventoryAccess.RegisterEvents();

            RExiled.Events.Handlers.Server.RoundStarted += RoundCharacterAssignmentManager.OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundEnded += RoundCharacterAssignmentManager.OnRoundEnded;
            RExiled.Events.Handlers.Player.Joined += RoundCharacterAssignmentManager.OnPlayerJoined;
            RExiled.Events.Handlers.Player.ChangedRole += RoundCharacterAssignmentManager.OnChangedRole;

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
            InventoryAccess.UnregisterEvents();

            RExiled.Events.Handlers.Server.RoundStarted -= RoundCharacterAssignmentManager.OnRoundStarted;
            RExiled.Events.Handlers.Server.RoundEnded -= RoundCharacterAssignmentManager.OnRoundEnded;
            RExiled.Events.Handlers.Player.Joined -= RoundCharacterAssignmentManager.OnPlayerJoined;
            RExiled.Events.Handlers.Player.ChangedRole -= RoundCharacterAssignmentManager.OnChangedRole;

            BadgeManager.SaveBadges();

            _webServer?.Dispose();
            _webServer = null;
        }
    }
}
