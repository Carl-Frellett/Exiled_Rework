using DreamPlugin.Badge;
using DreamPlugin.Game;
using DreamPlugin.Game.CustomRole;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;

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

            BadgeManager = new BadgeManager();
            BadgeManager.LoadBadges();

            _webServer = new WebServer();
            _webServer.Start();

            RoleSpawnManager.Register(new Scp6000Role());
            RoleSpawnManager.Register(new Scp550Role());
            RoleSpawnManager.Register(new Scp073Role());
            RoleSpawnManager.Register(new FatBRole());

            // 注册事件
            RExiled.Events.Handlers.Server.RoundStarted += RoleSpawnManager.OnRoundStarted;
            RExiled.Events.Handlers.Player.SpawnedTeam += RoleSpawnManager.OnSpawnedTeam;
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting += OnRACommand;
        }
        private void OnRACommand(RemoteAdminCommandExecutingEventArgs ev)
        {
            if (ev.Command.StartsWith("sp"))
            {
                string name = ev.Command.Substring(2);
                if (RoleSpawnManager.TrySpawnByCommand(name, ev.Player))
                {
                    ev.IsAllowed = false;
                }
            }
        }
        public override void OnDisabled()
        {
            base.OnDisabled();
            CommandHandler.UnregisterEvents();
            EventHandler.UnregisterEvents();
            InfiniteAmmo.UnregisterEvents();
            InventoryAccess.UnregisterEvents();

            BadgeManager.SaveBadges();

            _webServer?.Dispose();
            _webServer = null;

            RoleSpawnManager.UnregisterAll();
            RExiled.Events.Handlers.Server.RoundStarted -= RoleSpawnManager.OnRoundStarted;
            RExiled.Events.Handlers.Player.SpawnedTeam -= RoleSpawnManager.OnSpawnedTeam;
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting -= OnRACommand;
        }
    }
}
