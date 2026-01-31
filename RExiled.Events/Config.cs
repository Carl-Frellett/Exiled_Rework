using RExiled.API.Interfaces;
using System.ComponentModel;

namespace RExiled.Events
{
    public sealed class Config : IConfig
    {
        [Description("是否启用插件")]
        public bool IsEnabled { get; set; } = true;
    }
}
