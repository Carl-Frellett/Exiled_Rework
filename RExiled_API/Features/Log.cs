using System.Reflection;

namespace RExiled_API.Features
{
    public static class Log
    {
        public static void LogMessage(string message, bool isShowAssemblyName = false)
        {
            if (isShowAssemblyName)
            {
                ServerConsole.AddLog($"[{Assembly.GetCallingAssembly().GetName().Name}] {message}");
            }
            else
            {
                ServerConsole.AddLog(message);
            }
        }
    }
}
