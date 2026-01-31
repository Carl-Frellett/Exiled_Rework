using RExiled.Events.EventArgs.Server;
using RExiled.Events.Extensions;
using static RExiled.Events.Events;

namespace RExiled.Events.Handlers
{
    public static class Server
    {
        public static event CustomEventHandler<ServerConsoleCommandExecutingEventArgs> ServerConsoleCommandExecuting;

        public static void OnServerConsoleCommandExecuting(ServerConsoleCommandExecutingEventArgs ev) => ServerConsoleCommandExecuting.InvokeSafely(ev);
    }
}
