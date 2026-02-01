using System;

namespace RExiled.Events.EventArgs.Server
{
    public class ServerCommandExecutingEventArgs : System.EventArgs
    {
        public ServerCommandExecutingEventArgs(string command, bool isAllowed)
        {
            Command = command;
            IsAllowed = isAllowed;
        }

        public string Command { get; set; }
        public bool IsAllowed { get; set; }
    }
}