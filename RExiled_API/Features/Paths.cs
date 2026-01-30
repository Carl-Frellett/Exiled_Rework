using System;
using System.IO;

namespace RExiled.API.Features
{
    public static class Paths
    {
        static Paths() => Reload();

        public static string AppData { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string ManagedAssemblies { get; } = Path.Combine(Path.Combine(Environment.CurrentDirectory, "SCPSL_Data"), "Managed");

        public static string Exiled { get; set; }

        public static string Plugins { get; set; }

        public static string Dependencies { get; set; }

        public static string Configs { get; set; }

        public static string Config { get; set; }

        public static string Log { get; set; }

        public static void Reload(string rootDirectoryName = "EXILED")
        {
            Exiled = Path.Combine(AppData, rootDirectoryName);
            Plugins = Path.Combine(Exiled, "Plugins");
            Dependencies = Path.Combine(Plugins, "dependencies");
            Configs = Path.Combine(Exiled, "Configs");
            Config = Path.Combine(Configs, $"{Server.Port}-config.yml");
            Log = Path.Combine(Exiled, $"{Server.Port}-RemoteAdminLog.txt");
        }
    }
}
