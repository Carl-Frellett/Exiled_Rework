using RExiled.API.Interfaces;
using System.ComponentModel;

namespace RExiled.Events
{
    public class Config : IConfig
    {
        [Description("是否启用插件")]
        public bool IsEnabled { get; set; } = true;
    }
}
