using System.ComponentModel;
using RExiled.API.Interfaces;

namespace RExiled.Loader
{
    public sealed class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Indicates whether outdated plugins should be loaded or not")]
        public bool ShouldLoadOutdatedPlugins { get; set; } = true;

    }
}
