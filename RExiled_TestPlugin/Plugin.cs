using RExiled.API.Features;
using RExiled.Events.EventArgs;
using RExiled.Events.EventArgs.Player;
using RExiled.Events.EventArgs.Server;

namespace RExiled_TestPlugin
{
    public class Plugin : Plugin<Config>
    {
        public override void OnEnabled()
        {
            base.OnEnabled();
            RExiled.Events.Handlers.Player.Joined += OnPlayerJoin;
            RExiled.Events.Handlers.Player.Left += OnPlayerLeft;
            RExiled.Events.Handlers.Player.PlayerConsoleCommandExecuting += OnPlayerEnterCommand;
            RExiled.Events.Handlers.Player.RemoteAdminCommandExecuting += OnPlayerEnterCommandInRA;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
        }



        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            
        }

        public void OnPlayerLeft(LeftEventArgs ev)
        { 
        }

        public void OnPlayerEnterCommand(PlayerConsoleCommandExecutingEventArgs ev)
        { 
        }

        public void OnPlayerEnterCommandInRA(RemoteAdminCommandExecutingEventArgs ev)
        { 
        }
    }
}
