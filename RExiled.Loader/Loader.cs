using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RExiled.API.Features;
using RExiled.API.Interfaces;

namespace RExiled.Loader
{
    public static class Loader
    {
        static Loader()
        {
            Log.OK($"Initializing at {Environment.CurrentDirectory}");
            Log.OK($"{Assembly.GetExecutingAssembly().GetName().Name} - Version {Version.Major}.{Version.Minor}.{Version.Build}");

            CustomNetworkManager.Modded = true;

            if (!Directory.Exists(Paths.Configs))
                Directory.CreateDirectory(Paths.Configs);

            if (!Directory.Exists(Paths.Plugins))
                Directory.CreateDirectory(Paths.Plugins);

            if (!Directory.Exists(Paths.Dependencies))
                Directory.CreateDirectory(Paths.Dependencies);
        }

        public static List<IPlugin<IConfig>> Plugins { get; } = new List<IPlugin<IConfig>>();

        public static Random Random { get; } = new Random();

        public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        public static Config Config { get; } = new Config();
        public static List<Assembly> Dependencies { get; } = new List<Assembly>();
        public static void Run(Assembly[] dependencies = null)
        {
            if (dependencies != null && dependencies.Length > 0)
                Dependencies.AddRange(dependencies);

            LoadDependencies();
            LoadPlugins();

            ConfigManager.Reload();

            EnablePlugins();
        }

        public static void LoadPlugins()
        {
            foreach (string pluginPath in Directory.GetFiles(Paths.Plugins).Where(path => (path.EndsWith(".dll") || path.EndsWith(".exe")) && !IsAssemblyLoaded(path)))
            {
                Assembly assembly = LoadAssembly(pluginPath);

                if (assembly == null)
                    continue;

                IPlugin<IConfig> plugin = CreatePlugin(assembly);

                if (plugin == null)
                    continue;

                Plugins.Add(plugin);
            }

            Plugins.Sort();
        }

        public static Assembly LoadAssembly(string path)
        {
            try
            {
                return Assembly.Load(File.ReadAllBytes(path));
            }
            catch (Exception exception)
            {
                Log.Error($"Error while loading a plugin at {path}! {exception}");
            }

            return null;
        }

        public static IPlugin<IConfig> CreatePlugin(Assembly assembly)
        {
            try
            {
                foreach (Type type in assembly.GetTypes().Where(type => !type.IsAbstract && !type.IsInterface))
                {
                    if (!type.BaseType.IsGenericType || type.BaseType.GetGenericTypeDefinition() != typeof(Plugin<>))
                    {
                        continue;
                    }


                    IPlugin<IConfig> plugin = null;

                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {

                        plugin = constructor.Invoke(null) as IPlugin<IConfig>;
                    }
                    else
                    {

                        var value = Array.Find(type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public), property => property.PropertyType == type)?.GetValue(null);

                        if (value != null)
                            plugin = value as IPlugin<IConfig>;
                    }

                    if (plugin == null)
                    {
                        Log.Error($"{type.FullName} is a valid plugin, but it cannot be instantiated! It either doesn't have a public default constructor without any arguments or a static property of the {type.FullName} type!");

                        continue;
                    }


                    if (plugin.RequiredExiledVersion > Version)
                    {
                        if (!Config.ShouldLoadOutdatedPlugins)
                        {
                            Log.Error($"You're running an older version of Exiled ({Version.ToString(3)})! {plugin.Name} won't be loaded! " +
                            $"Required version to load it: {plugin.RequiredExiledVersion.ToString(3)}");

                            continue;
                        }
                        else
                        {
                            Log.Warn($"You're running an older version of Exiled ({Version.ToString(3)})! " +
                            $"You may encounter some bugs by loading {plugin.Name}! Update Exiled to at least {plugin.RequiredExiledVersion.ToString(3)}");
                        }
                    }

                    return plugin;
                }
            }
            catch (Exception exception)
            {
                Log.Error($"Error while initializing plugin {assembly.GetName().Name} (at {assembly.Location})! {exception}");
            }

            return null;
        }

        public static void EnablePlugins()
        {
            foreach (IPlugin<IConfig> plugin in Plugins)
            {
                try
                {
                    if (plugin.Config.IsEnabled)
                    {
                        plugin.OnEnabled();
                    }
                }
                catch (Exception exception)
                {
                    Log.Error($"Plugin \"{plugin.Name}\" threw an exception while enabling: {exception}");
                }
            }
        }

        public static void ReloadPlugins()
        {
            foreach (IPlugin<IConfig> plugin in Plugins)
            {
                try
                {
                    plugin.OnReloaded();

                    plugin.Config.IsEnabled = false;

                    plugin.OnDisabled();
                }
                catch (Exception exception)
                {
                    Log.Error($"Plugin \"{plugin.Name}\" threw an exception while reloading: {exception}");
                }
            }

            LoadPlugins();

            ConfigManager.Reload();

            EnablePlugins();
        }

        public static void DisablePlugins()
        {
            foreach (IPlugin<IConfig> plugin in Plugins)
            {
                try
                {
                    plugin.Config.IsEnabled = false;
                    plugin.OnDisabled();
                }
                catch (Exception exception)
                {
                    Log.Error($"Plugin \"{plugin.Name}\" threw an exception while disabling: {exception}");
                }
            }
        }

        public static bool IsDependencyLoaded(string path) => Dependencies.Exists(assembly => assembly.Location == path);

        public static bool IsAssemblyLoaded(string path) => Plugins.Any(plugin => plugin.Assembly.Location == path);

        private static void LoadDependencies()
        {
            try
            {
                Log.Info($"Loading dependencies at {Paths.Dependencies}");

                foreach (string dependency in Directory.GetFiles(Paths.Dependencies).Where(path => path.EndsWith(".dll") && !IsDependencyLoaded(path)))
                {
                    Assembly assembly = LoadAssembly(dependency);

                    if (assembly == null)
                        continue;

                    Dependencies.Add(assembly);

                    Log.Info($"Loaded dependency {assembly.FullName}");
                }

                Log.Info("Dependencies loaded successfully!");
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while loading dependencies! {exception}");
            }
        }
    }
}
