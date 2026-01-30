using RExiled.API.Enums;
using System;
using System.Reflection;

namespace RExiled.API.Interfaces
{
    // Plugin 类接口类
    public interface IPlugin<out TConfig> : IComparable<IPlugin<IConfig>>
        where TConfig : IConfig
    {
        Assembly Assembly { get; }

        string Name { get; }

        string Prefix { get; }

        string Author { get; }

        PluginPriority Priority { get; }

        Version Version { get; }

        Version RequiredExiledVersion { get; }

        TConfig Config { get; }

        void OnEnabled();

        void OnDisabled();

        void OnReloaded();
    }
}
