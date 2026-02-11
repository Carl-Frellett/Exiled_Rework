using Mirror;
using RExiled.API.Features;
using UnityEngine;

namespace RExiled.API.Extensions
{
    public static class Item
    {
        public static Pickup Spawn(this ItemType itemType, float durability, Vector3 position, Quaternion rotation = default, int sight = 0, int barrel = 0, int other = 0) => Server.Host.Inventory.SetPickup(itemType, durability, position, rotation, sight, barrel, other);

        public static void SetWeaponAmmo(this Inventory.SyncListItemInfo list, Inventory.SyncItemInfo weapon, int amount) => list.ModifyDuration(list.IndexOf(weapon), amount);

        public static void SetWeaponAmmo(this RExiled.API.Features.Player player, Inventory.SyncItemInfo weapon, int amount) => player.Inventory.items.ModifyDuration(player.Inventory.items.IndexOf(weapon), amount);

        public static float GetWeaponAmmo(this Inventory.SyncItemInfo weapon) => weapon.durability;

        public static bool IsAmmo(this ItemType item) => item == ItemType.Ammo556 || item == ItemType.Ammo9mm || item == ItemType.Ammo762;

        public static bool IsWeapon(this ItemType type, bool checkMicro = true) =>
            type == ItemType.GunCOM15 || type == ItemType.GunE11SR || type == ItemType.GunLogicer ||
            type == ItemType.GunMP7 || type == ItemType.GunProject90 || type == ItemType.GunUSP || (checkMicro && type == ItemType.MicroHID);

        public static bool IsSCP(this ItemType type) => type == ItemType.SCP018 || type == ItemType.SCP500 || type == ItemType.SCP268 || type == ItemType.SCP207;

        public static bool IsThrowable(this ItemType type) => type == ItemType.SCP018 || type == ItemType.GrenadeFrag || type == ItemType.GrenadeFlash;

        public static bool IsMedical(this ItemType type) => type == ItemType.Painkillers || type == ItemType.Medkit || type == ItemType.SCP500 || type == ItemType.Adrenaline;

        public static bool IsUtility(this ItemType type) => type == ItemType.Disarmer || type == ItemType.Flashlight || type == ItemType.Radio || type == ItemType.WeaponManagerTablet;

        public static bool IsKeycard(this ItemType type) =>
            type == ItemType.KeycardChaosInsurgency || type == ItemType.KeycardContainmentEngineer || type == ItemType.KeycardFacilityManager ||
            type == ItemType.KeycardGuard || type == ItemType.KeycardJanitor || type == ItemType.KeycardNTFCommander ||
            type == ItemType.KeycardNTFLieutenant || type == ItemType.KeycardO5 || type == ItemType.KeycardScientist ||
            type == ItemType.KeycardScientistMajor || type == ItemType.KeycardSeniorGuard || type == ItemType.KeycardZoneManager;

        public static void SyncPickupSize(this Pickup pickup)
        {
            var pickupObject = pickup.gameObject;
            NetworkIdentity identity = pickupObject.GetComponent<NetworkIdentity>();

            ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
            destroyMessage.netId = identity.netId;

            foreach (GameObject player in PlayerManager.players)
            {
                Mirror.NetworkConnection playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;
                playerCon.Send(destroyMessage, 0);
                object[] parameters = new object[] { identity, playerCon };
                typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", parameters);
            }
        }
    }
}
