namespace DreamPlugin.Game.CustomRole.Extensions
{
    public static class RoleTypeExtensions
    {
        public static bool IsNTF(this RoleType role)
        {
            return role == RoleType.NtfCadet ||
                   role == RoleType.NtfLieutenant ||
                   role == RoleType.NtfCommander ||
                   role == RoleType.NtfScientist;
        }

        public static bool IsChaos(this RoleType role)
        {
            return role == RoleType.ChaosInsurgency;
        }

        public static bool IsScp(this RoleType role)
        {
            return role == RoleType.Scp173 ||
                   role == RoleType.Scp106 ||
                   role == RoleType.Scp049 ||
                   role == RoleType.Scp079 ||
                   role == RoleType.Scp096 ||
                   role == RoleType.Scp0492 ||
                   role == RoleType.Scp93953 ||
                   role == RoleType.Scp93989;
        }

        public static bool IsAllowedInRoundStart(this RoleType role)
        {
            return !role.IsNTF() && !role.IsChaos();
        }
    }
}