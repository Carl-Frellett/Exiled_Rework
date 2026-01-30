using System.Reflection;

namespace RExiled.API.Features
{
    public static class Log
    {
        public static void SendRaw(string message)
        {
            ServerConsole.AddLog($"[Info] {message}");
        }
        public static void Info(string message)
        {
            ServerConsole.AddLog($"[Info] [{Assembly.GetCallingAssembly().GetName().Name}] {message}");
        }

        public static void Debug(string message)
        {
#if Debug
            ServerConsole.AddLog($"[DEBUG] [{Assembly.GetCallingAssembly().GetName().Name}] {message} LOGTYPE02");
#endif
        }

        public static void Warn(string message)
        {
            ServerConsole.AddLog($"[Warning] [{Assembly.GetCallingAssembly().GetName().Name}] {message} LOGTYPE14");
        }

        public static void Error(string message)
        {
            ServerConsole.AddLog($"[ERROR] [{Assembly.GetCallingAssembly().GetName().Name}] {message} LOGTYPE-8");
        }
    }
}
