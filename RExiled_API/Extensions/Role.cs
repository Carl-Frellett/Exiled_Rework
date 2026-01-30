using RExiled.API.Enums;

using UnityEngine;

namespace RExiled.API.Extensions
{
    public static class Role
    {
        public static Color GetColor(this RoleType role) => role == RoleType.None ? Color.white : CharacterClassManager._staticClasses.Get(role).classColor;

        public static Side GetSide(this RoleType role) =>
            role.GetTeam().GetSide();

        public static Side GetSide(this Team team)
        {
            switch (team)
            {
                case Team.SCP:
                    return Side.SCP;
                case Team.MTF:
                case Team.RSC:
                    return Side.MTF;
                case Team.CHI:
                case Team.CDP:
                    return Side.CHAOS;
                case Team.TUT:
                    return Side.TUTORIAL;
                case Team.RIP:
                default:
                    return Side.NONE;
            }
        }

        /// <summary>
        /// Get the <see cref="Team"/> of the given <see cref="RoleType"/>.
        /// </summary>
        /// <param name="roleType">Role.</param>
        /// <returns><see cref="Team"/>.</returns>
        public static Team GetTeam(this RoleType roleType)
        {
            switch (roleType)
            {
                case RoleType.ChaosInsurgency:
                    return Team.CHI;
                case RoleType.Scientist:
                    return Team.RSC;
                case RoleType.ClassD:
                    return Team.CDP;
                case RoleType.Scp049:
                case RoleType.Scp93953:
                case RoleType.Scp93989:
                case RoleType.Scp0492:
                case RoleType.Scp079:
                case RoleType.Scp096:
                case RoleType.Scp106:
                case RoleType.Scp173:
                    return Team.SCP;
                case RoleType.Spectator:
                    return Team.RIP;
                case RoleType.FacilityGuard:
                case RoleType.NtfCadet:
                case RoleType.NtfLieutenant:
                case RoleType.NtfCommander:
                case RoleType.NtfScientist:
                    return Team.MTF;
                case RoleType.Tutorial:
                    return Team.TUT;
                default:
                    return Team.RIP;
            }
        }
    }
}
