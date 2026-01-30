namespace RExiled.Loader
{
    using Exiled.API.Extensions;
    using RExiled.API.Extensions;
    using RExiled.API.Features;
    using RExiled.API.Interfaces;
    using RExiled.Loader.Features.Configs;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.NodeDeserializers;

    public static class ConfigManager
    {
        public static ISerializer Serializer { get; } = new SerializerBuilder()
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreFields()
            .Build();

        public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), deserializer => deserializer.InsteadOf<ObjectNodeDeserializer>())
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .Build();

        public static Dictionary<string, IConfig> Load(string rawConfigs)
        {
            try
            {
                Log.Info("Loading plugin configs...");

                rawConfigs = Regex.Replace(rawConfigs, @"\ !.*", string.Empty).Replace("!Dictionary[string,IConfig]", string.Empty);

                Dictionary<string, object> rawDeserializedConfigs = Deserializer.Deserialize<Dictionary<string, object>>(rawConfigs) ?? new Dictionary<string, object>();
                Dictionary<string, IConfig> deserializedConfigs = new Dictionary<string, IConfig>();

                if (!rawDeserializedConfigs.TryGetValue("exiled_loader", out object rawDeserializedConfig))
                {
                    Log.Warn($"Exiled.Loader doesn't have default configs, generating...");

                    deserializedConfigs.Add("exiled_loader", Loader.Config);
                }
                else
                {
                    deserializedConfigs.Add("exiled_loader", Deserializer.Deserialize<Config>(Serializer.Serialize(rawDeserializedConfig)));

                    Loader.Config.CopyProperties(deserializedConfigs["exiled_loader"]);
                }

                foreach (IPlugin<IConfig> plugin in Loader.Plugins)
                {
                    if (!rawDeserializedConfigs.TryGetValue(plugin.Prefix, out rawDeserializedConfig))
                    {
                        Log.Warn($"{plugin.Name} doesn't have default configs, generating...");

                        deserializedConfigs.Add(plugin.Prefix, plugin.Config);
                    }
                    else
                    {
                        try
                        {
                            deserializedConfigs.Add(plugin.Prefix, (IConfig)Deserializer.Deserialize(Serializer.Serialize(rawDeserializedConfig), plugin.Config.GetType()));

                            plugin.Config.CopyProperties(deserializedConfigs[plugin.Prefix]);
                        }
                        catch (YamlException yamlException)
                        {
                            Log.Error($"{plugin.Name} configs could not be loaded, some of them are in a wrong format, default configs will be loaded instead! {yamlException}");

                            deserializedConfigs.Add(plugin.Prefix, plugin.Config);
                        }
                    }
                }

                Log.Info("Plugin configs loaded successfully!");

                return deserializedConfigs;
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while loading configs! {exception}");

                return null;
            }
        }

        public static bool Reload() => Save(Load(Read()));

        public static bool Save(string configs)
        {
            try
            {
                File.WriteAllText(Paths.Config, configs ?? string.Empty);

                return true;
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while saving configs to {Paths.Config} path: {exception}");

                return false;
            }
        }

        public static bool Save(Dictionary<string, IConfig> configs)
        {
            try
            {
                if (configs == null || configs.Count == 0)
                    return false;

                return Save(Serializer.Serialize(configs));
            }
            catch (YamlException yamlException)
            {
                Log.Error($"An error has occurred while serializing configs: {yamlException}");

                return false;
            }
        }
        public static string Read()
        {
            try
            {
                if (File.Exists(Paths.Config))
                    return File.ReadAllText(Paths.Config);
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while reading configs from {Paths.Config} path: {exception}");
            }

            return string.Empty;
        }
        public static bool Clear() => Save(string.Empty);

        public static void ReloadRemoteAdmin()
        {
            ServerStatic.RolesConfig = new YamlConfig(ServerStatic.RolesConfigPath);
            ServerStatic.SharedGroupsConfig = (GameCore.ConfigSharing.Paths[4] == null) ? null : new YamlConfig(GameCore.ConfigSharing.Paths[4] + "shared_groups.txt");
            ServerStatic.SharedGroupsMembersConfig = (GameCore.ConfigSharing.Paths[5] == null) ? null : new YamlConfig(GameCore.ConfigSharing.Paths[5] + "shared_groups_members.txt");
            ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
            ServerStatic.GetPermissionsHandler().RefreshPermissions();

            foreach (Player p in Player.List)
            {
                p.ReferenceHub.serverRoles.SetGroup(null, false, false, false);
                p.ReferenceHub.serverRoles.RefreshPermissions();
            }
        }
    }
}
