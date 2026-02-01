using System;
using System.IO;
using System.Reflection;

namespace RExiled.Bootstrap
{
    public sealed class Bootstrap
    {
        public static bool IsLoaded { get; private set; }

        public static void Load()
        {
            if (IsLoaded)
            {
                ServerConsole.AddLog("[RExiled.Bootstrap] RExiled has already been loaded! LOGTYPE10");
                return;
            }

            try
            {
                ServerConsole.AddLog("[RExiled.Bootstrap] RExiled is loading... LOGTYPE10");

                string exiledRoot = Path.Combine(Environment.CurrentDirectory, "EXILED");
                exiledRoot = Path.Combine(Environment.CurrentDirectory, "EXILED");

                string loaderPath = Path.Combine(exiledRoot, "RExiled.Loader.dll");
                string pluginsDir = Path.Combine(exiledRoot, "Plugins");
                string dependenciesDir = Path.Combine(pluginsDir, "dependencies");

                string apiPath = Path.Combine(dependenciesDir, "RExiled.API.dll");
                string yamlPath = Path.Combine(dependenciesDir, "YamlDotNet.dll");

                if (!File.Exists(loaderPath))
                {
                    ServerConsole.AddLog($"[RExiled.Bootstrap] RExiled.Loader.dll was not found at {loaderPath}, RExiled won't be loaded! LOGTYPE4");
                    return;
                }

                if (!File.Exists(apiPath))
                {
                    ServerConsole.AddLog($"[RExiled.Bootstrap] RExiled.API.dll was not found at {apiPath}, RExiled won't be loaded! LOGTYPE4");
                    return;
                }

                if (!File.Exists(yamlPath))
                {
                    ServerConsole.AddLog($"[RExiled.Bootstrap] YamlDotNet.dll was not found at {yamlPath}, RExiled won't be loaded! LOGTYPE4");
                    return;
                }

                Directory.CreateDirectory(pluginsDir);
                Directory.CreateDirectory(dependenciesDir);
                Directory.CreateDirectory(Path.Combine(exiledRoot, "Configs"));

                var apiAssembly = Assembly.Load(File.ReadAllBytes(apiPath));
                var yamlAssembly = Assembly.Load(File.ReadAllBytes(yamlPath));

                Assembly loaderAssembly = Assembly.Load(File.ReadAllBytes(loaderPath));
                var loaderType = loaderAssembly.GetType("RExiled.Loader.Loader");
                var runMethod = loaderType?.GetMethod("Run");

                runMethod?.Invoke(null, new object[]
                {
                    new Assembly[] { apiAssembly, yamlAssembly }
                });

                IsLoaded = true;
                ServerConsole.AddLog("[RExiled.Bootstrap] RExiled loaded successfully!");
            }
            catch (Exception exception)
            {
                ServerConsole.AddLog($"[RExiled.Bootstrap] Exiled loading error: {exception} LOGTYPE4");
            }
        }
    }
}