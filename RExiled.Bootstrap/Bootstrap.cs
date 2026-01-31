namespace RExiled.Bootstrap
{
    using System;
    using System.IO;
    using System.Reflection;

    public sealed class Bootstrap
    {
        public static bool IsLoaded { get; private set; }

        public static void Load()
        {
            if (IsLoaded)
            {
                ServerConsole.AddLog("[RExiled.Bootstrap] RExiled has already been loaded! LOGTYPE2");
                return;
            }

            try
            {
                ServerConsole.AddLog("[RExiled.Bootstrap] RExiled is loading...");

                string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED");
                string dependenciesPath = Path.Combine(rootPath, "Plugins", "dependencies");

                if (Environment.CurrentDirectory.Contains("testing", StringComparison.OrdinalIgnoreCase))
                    rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED-Testing");

                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);

                if (File.Exists(Path.Combine(rootPath, "RExiled.Loader.dll")))
                {
                    if (File.Exists(Path.Combine(dependenciesPath, "RExiled.API.dll")))
                    {
                        if (File.Exists(Path.Combine(dependenciesPath, "YamlDotNet.dll")))
                        {
                            Assembly.Load(File.ReadAllBytes(Path.Combine(rootPath, "RExiled.Loader.dll")))
                                .GetType("RExiled.Loader.Loader")
                                .GetMethod("Run")
                                ?.Invoke(
                                    null,
                                    new object[]
                                    {
                                        new Assembly[]
                                        {
                                            Assembly.Load(File.ReadAllBytes(Path.Combine(dependenciesPath, "RExiled.API.dll"))),
                                            Assembly.Load(File.ReadAllBytes(Path.Combine(dependenciesPath, "YamlDotNet.dll"))),
                                        },
                                    });

                            IsLoaded = true;
                        }
                        else
                        {
                            ServerConsole.AddLog($"[RExiled.Bootstrap] YamlDotNet.dll was not found, RExiled won't be loaded! LOGTYPE4");
                        }
                    }
                    else
                    {
                        ServerConsole.AddLog($"[RExiled.Bootstrap] RExiled.API.dll was not found, RExiled won't be loaded! LOGTYPE4");
                    }
                }
                else
                {
                    ServerConsole.AddLog($"[RExiled.Bootstrap] RExiled.Loader.dll was not found, RExiled won't be loaded! LOGTYPE4");
                }
            }
            catch (Exception exception)
            {
                ServerConsole.AddLog($"[RExiled.Bootstrap] Exiled loading error: {exception} LOGTYPE4");
            }
        }
    }
}
