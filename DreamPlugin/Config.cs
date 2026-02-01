using RExiled.API.Features;
using RExiled.API.Interfaces;
using System.ComponentModel;
using System.IO;

namespace DreamPlugin
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("管理员密码")]
        public string AdminPwd { get; set; } = "admin123";

        [Description("称号系统: 称号数据库")]
        public string BadgeDataPath { get; set; } = Path.Combine(Paths.Configs, "Badges.json");

        [Description("称号系统: 颜色变换频率")]
        public float ColorChangeInterval { get; set; } = 0.5f;

        [Description("称号系统: 内容变换频率")]
        public float TextChangeInterval { get; set; } = 0.4f;
    }
}
