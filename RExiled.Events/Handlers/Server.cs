using RExiled.Events.EventArgs.Server;
using RExiled.Events.Extensions;
using static RExiled.Events.Events;

namespace RExiled.Events.Handlers
{
    public static class Server
    {

        public static event CustomEventHandler RoundStarted;

        public static event CustomEventHandler RoundRestarted; 
        
        public static event CustomEventHandler RoundEnded;

        public static event CustomEventHandler<ServerCommandExecutingEventArgs> ServerCommandExecuting;

        public static event CustomEventHandler WaitingForPlayers;

        public static void OnWaitingForPlayers() => WaitingForPlayers?.InvokeSafely();

        public static void OnTerminalCommandExecuting(ServerCommandExecutingEventArgs ev)=> ServerCommandExecuting.InvokeSafely(ev);

        public static void OnRoundEnded() => RoundEnded.InvokeSafely();

        public static void OnRoundRestarted() => RoundRestarted.InvokeSafely();

        public static void OnRoundStarted() => RoundStarted.InvokeSafely();
    }
}
