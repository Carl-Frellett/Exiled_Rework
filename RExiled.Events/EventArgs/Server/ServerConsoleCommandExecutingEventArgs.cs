using System;

namespace RExiled.Events.EventArgs.Server
{
    public class ServerConsoleCommandExecutingEventArgs : System.EventArgs
    {
        public ServerConsoleCommandExecutingEventArgs(string command, bool encrypted, bool isAllowed)
        {
            Command = command;
            Encrypted = encrypted;
            IsAllowed = isAllowed;
            Response = string.Empty;
            Color = "white";
        }

        public string Command { get; set; }
        public bool Encrypted { get; set; }
        public bool IsAllowed { get; set; }
        public string Response { get; set; }
        public string Color { get; set; }
    }
}