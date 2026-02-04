using DreamPlugin.Badge;
using DreamPlugin.Game;
using DreamPlugin.Game.PlayerRole;
using MEC;
using RExiled.API.Features;

namespace DreamPlugin
{
    public class Plugin : Plugin<Config>
    {
        // 燕然是啥骚气比
        public override string Name => "DreamPlugin";
        public override string Author => "Carl Frellett";

        public static Plugin plugin { get; private set; }

        private CommandHandler CommandHandler = new CommandHandler();
        private EventHandler EventHandler = new EventHandler();
        private InfiniteAmmo InfiniteAmmo = new InfiniteAmmo();
        private SCP073 SCP073 = new SCP073();
        private SCP550 SCP550 = new SCP550();

        private BigGuard BigGuard = new BigGuard();

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

            BadgeManager = new BadgeManager();
            BadgeManager.LoadBadges();

            Timing.CallDelayed(5f, () =>
            {
                _webServer = new WebServer();
                _webServer.Start();
            });
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            CommandHandler.UnregisterEvents();
            EventHandler.UnregisterEvents();
            InfiniteAmmo.UnregisterEvents();
            BigGuard.UnregisterEvents();
            SCP073.UnregisterEvents();
            SCP550 = new SCP550();

            BadgeManager.SaveBadges();

            _webServer?.Dispose();
            _webServer = null;
        }
    }
}
