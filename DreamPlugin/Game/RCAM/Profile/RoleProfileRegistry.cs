using RExiled.API.Enums;
using RExiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreamPlugin.Game.RCAM.Profile
{
    public static class RoleProfileRegistry
    {
        private static readonly Dictionary<RoleType, RoleProfile> Profiles = new Dictionary<RoleType, RoleProfile>();

        static RoleProfileRegistry()
        {
            InitializeProfiles();
        }

        public static RoleProfile GetProfile(RoleType role)
        {
            return Profiles.TryGetValue(role, out var profile) ? profile : null;
        }

        private static void InitializeProfiles()
        {
            Profiles[RoleType.ClassD] = new RoleProfile
            {
                Health = 100,
                MaxHealth = 100,
                StartingItems = new List<ItemType>
                {
                    ItemType.KeycardJanitor,
                    ItemType.Adrenaline,
                    ItemType.Flashlight
                }
            };

            Profiles[RoleType.Scientist] = new RoleProfile
            {
                Health = 100,
                MaxHealth = 100,
                StartingItems = new List<ItemType>
                {
                    ItemType.KeycardScientist,
                    ItemType.Medkit,
                    ItemType.Adrenaline,
                    ItemType.Flashlight
                }
            };

            Profiles[RoleType.FacilityGuard] = new RoleProfile
            {
                Health = 120,
                MaxHealth = 120,
                StartingItems = new List<ItemType>
                {
                    ItemType.GunMP7,
                    ItemType.KeycardGuard,
                    ItemType.WeaponManagerTablet,
                    ItemType.Radio,
                    ItemType.Disarmer,
                    ItemType.Medkit,
                    ItemType.Adrenaline,
                    ItemType.GrenadeFlash,
                }
            };

            Profiles[RoleType.ChaosInsurgency] = new RoleProfile
            {
                Health = 135,
                MaxHealth = 135,
                StartingItems = new List<ItemType>
                {
                    ItemType.GunLogicer,
                    ItemType.GunUSP,
                    ItemType.KeycardChaosInsurgency,
                    ItemType.WeaponManagerTablet,
                    ItemType.GrenadeFrag,
                    ItemType.Medkit,
                    ItemType.Painkillers
                }
            };

            Profiles[RoleType.NtfCommander] = new RoleProfile
            {
                Health = 150,
                MaxHealth = 150,
                StartingItems = new List<ItemType>
                {
                    ItemType.GunE11SR,
                    ItemType.GunUSP,
                    ItemType.GrenadeFrag,
                    ItemType.KeycardNTFCommander,
                    ItemType.WeaponManagerTablet,
                    ItemType.Radio,
                    ItemType.Disarmer,
                    ItemType.Medkit
                }
            };

            Profiles[RoleType.NtfLieutenant] = new RoleProfile
            {
                Health = 120,
                MaxHealth = 120,
                StartingItems = new List<ItemType>
                {
                    ItemType.GunE11SR,
                    ItemType.GunUSP,
                    ItemType.GrenadeFrag,
                    ItemType.KeycardNTFLieutenant,
                    ItemType.WeaponManagerTablet,
                    ItemType.Radio,
                    ItemType.Disarmer,
                    ItemType.Medkit
                }
            };

            Profiles[RoleType.NtfScientist] = new RoleProfile
            {
                Health = 120,
                MaxHealth = 120,
                StartingItems = new List<ItemType>
                {
                    ItemType.GunE11SR,
                    ItemType.GunUSP,
                    ItemType.GrenadeFrag,
                    ItemType.KeycardNTFLieutenant,
                    ItemType.WeaponManagerTablet,
                    ItemType.Radio,
                    ItemType.Disarmer,
                    ItemType.Medkit
                }
            };

            Profiles[RoleType.NtfCadet] = new RoleProfile
            {
                Health = 120,
                MaxHealth = 120,
                StartingItems = new List<ItemType>
                {
                    ItemType.GunProject90,
                    ItemType.GunUSP,
                    ItemType.KeycardNTFLieutenant,
                    ItemType.WeaponManagerTablet,
                    ItemType.Radio,
                    ItemType.Disarmer,
                    ItemType.Medkit
                }
            };

            Profiles[RoleType.Scp93953] = new RoleProfile
            {
                Health = 3200,
                MaxHealth = 3200,
            };

            Profiles[RoleType.Scp93989] = new RoleProfile
            {
                Health = 3200,
                MaxHealth = 3200,
            };

            Profiles[RoleType.Scp096] = new RoleProfile
            {
                Health = 2800,
                MaxHealth = 2800,
            };

            Profiles[RoleType.Scp049] = new RoleProfile
            {
                Health = 2500,
                MaxHealth = 2500,
            };

            Profiles[RoleType.Scp173] = new RoleProfile
            {
                Health = 4150,
                MaxHealth = 4150,
            };

            Profiles[RoleType.Scp106] = new RoleProfile
            {
                Health = 700,
                MaxHealth = 700,
            };

            Profiles[RoleType.Scp0492] = new RoleProfile
            {
                Health = 500,
                MaxHealth = 500,
            };
        }
    }
}
