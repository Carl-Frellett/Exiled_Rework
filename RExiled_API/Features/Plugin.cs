using System;
using System.Reflection;

using RExiled.API.Enums;
using RExiled.API.Extensions;
using RExiled.API.Interfaces;

namespace RExiled.API.Features
{
    public abstract class Plugin<TConfig> : IPlugin<TConfig>
        where TConfig : IConfig, new()
    {
        public Plugin()
        {
            Name = Assembly.GetName().Name;
            Prefix = Name.ToSnakeCase();
            Author = Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            Version = Assembly.GetName().Version;
        }

        public Assembly Assembly { get; } = Assembly.GetCallingAssembly();

        public virtual string Name { get; }

        public virtual string Prefix { get; }

        public virtual string Author { get; }

        public virtual PluginPriority Priority { get; }

        public virtual Version Version { get; }

        public virtual Version RequiredExiledVersion { get; } = typeof(IPlugin<>).Assembly.GetName().Version;

        public TConfig Config { get; } = new TConfig();

        public virtual void OnEnabled() => Log.Info($"{Name} v{Version.Major}.{Version.Minor}.{Version.Build}, made by {Author}, has been enabled!");

        public virtual void OnDisabled() => Log.Info($"{Name} has been disabled!");

        public virtual void OnReloaded() => Log.Info($"{Name} has been reloaded!");

        public int CompareTo(IPlugin<IConfig> other) => -Priority.CompareTo(other.Priority);
    }
}
