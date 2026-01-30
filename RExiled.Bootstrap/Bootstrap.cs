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
                ServerConsole.AddLog("[Exiled.Bootstrap] Exiled has already been loaded!");
                return;
            }

            try
            {
                ServerConsole.AddLog("[Exiled.Bootstrap] Exiled is loading...");

                string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED");
                string dependenciesPath = Path.Combine(rootPath, "Plugins", "dependencies");

                if (Environment.CurrentDirectory.Contains("testing", StringComparison.OrdinalIgnoreCase))
                    rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED-Testing");

                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);

                if (File.Exists(Path.Combine(rootPath, "Exiled.Loader.dll")))
                {
                    if (File.Exists(Path.Combine(dependenciesPath, "Exiled.API.dll")))
                    {
                        if (File.Exists(Path.Combine(dependenciesPath, "YamlDotNet.dll")))
                        {
                            Assembly.Load(File.ReadAllBytes(Path.Combine(rootPath, "Exiled.Loader.dll")))
                                .GetType("Exiled.Loader.Loader")
                                .GetMethod("Run")
                                ?.Invoke(
                                    null,
                                    new object[]
                                    {
                                        new Assembly[]
                                        {
                                            Assembly.Load(File.ReadAllBytes(Path.Combine(dependenciesPath, "Exiled.API.dll"))),
                                            Assembly.Load(File.ReadAllBytes(Path.Combine(dependenciesPath, "YamlDotNet.dll"))),
                                        },
                                    });

                            IsLoaded = true;
                        }
                        else
                        {
                            ServerConsole.AddLog($"[Exiled.Bootstrap] YamlDotNet.dll was not found, Exiled won't be loaded!");
                        }
                    }
                    else
                    {
                        ServerConsole.AddLog($"[Exiled.Bootstrap] Exiled.API.dll was not found, Exiled won't be loaded!");
                    }
                }
                else
                {
                    ServerConsole.AddLog($"[Exiled.Bootstrap] Exiled.Loader.dll was not found, Exiled won't be loaded!");
                }
            }
            catch (Exception exception)
            {
                ServerConsole.AddLog($"[Exiled.Bootstrap] Exiled loading error: {exception}");
            }
        }
    }
}
