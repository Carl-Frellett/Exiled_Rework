using System.Reflection;

namespace RExiled.API.Features
{
    public static class Log
    {
        public static void SendRaw(string message)
        {
            ServerConsole.AddLog($"{message}");
        }
        public static void Info(string message)
        {
            ServerConsole.AddLog($"[Info] [{Assembly.GetCallingAssembly().GetName().Name}] {message} LOGTYPE11");
        }

        public static void Debug(string message)
        {
#if DEBUG
            ServerConsole.AddLog($"[DEBUG] [{Assembly.GetCallingAssembly().GetName().Name}] {message} LOGTYPE6");
#endif
        }

        public static void Warn(string message)
        {
            ServerConsole.AddLog($"[Warning] [{Assembly.GetCallingAssembly().GetName().Name}] {message} LOGTYPE5");
        }

        public static void Error(string message)
        {
            ServerConsole.AddLog($"[ERROR] [{Assembly.GetCallingAssembly().GetName().Name}] {message} LOGTYPE4");
        }

        public static void OK(string message)
        {
            ServerConsole.AddLog($"[OK] [{Assembly.GetCallingAssembly().GetName().Name}] {message} LOGTYPE10");
        }
    }
}
