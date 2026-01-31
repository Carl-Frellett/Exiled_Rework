using RExiled.Events.EventArgs.Server;
using RExiled.Events.Extensions;
using static RExiled.Events.Events;

namespace RExiled.Events.Handlers
{
    public static class Server
    {
        public static event CustomEventHandler<ServerConsoleCommandExecutingEventArgs> ServerConsoleCommandExecuting;

        public static event CustomEventHandler RoundStarted;

        public static event CustomEventHandler RoundRestarted; 
        
        public static event CustomEventHandler RoundEnded;

        public static void OnRoundEnded() => RoundEnded.InvokeSafely();

        public static void OnRoundRestarted() => RoundRestarted.InvokeSafely();

        public static void OnRoundStarted() => RoundStarted.InvokeSafely();

        public static void OnServerConsoleCommandExecuting(ServerConsoleCommandExecutingEventArgs ev) => ServerConsoleCommandExecuting.InvokeSafely(ev);
    }
}
