using Grenades;
using MEC;
using Mirror;
using RExiled.API.Enums;
using RExiled.API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RExiled.API.Features
{
    /// <summary>
    /// 表示服务器中的一个玩家。
    /// 该类封装了 ReferenceHub 的大部分功能，提供更简洁、安全且面向插件开发者的 API。
    /// </summary>
    public class Player
    {
        #region 构造函数

        private ReferenceHub referenceHub;

        /// <summary>
        /// 使用指定的 <see cref="ReferenceHub"/> 初始化 <see cref="Player"/> 实例。
        /// </summary>
        /// <param name="referenceHub">关联的 ReferenceHub。</param>
        public Player(ReferenceHub referenceHub) => ReferenceHub = referenceHub;

        /// <summary>
        /// 使用指定的 <see cref="GameObject"/> 初始化 <see cref="Player"/> 实例。
        /// </summary>
        /// <param name="gameObject">玩家的游戏对象。</param>
        public Player(GameObject gameObject) => ReferenceHub = ReferenceHub.GetHub(gameObject);

        /// <summary>
        /// 获取或设置此玩家关联的 <see cref="ReferenceHub"/>。
        /// 设置时会自动初始化相关组件（如 Ammo、Inventory 等）。
        /// </summary>
        /// <exception cref="NullReferenceException">当传入 null 时抛出。</exception>
        public ReferenceHub ReferenceHub
        {
            get => referenceHub;
            private set
            {
                if (value == null)
                    throw new NullReferenceException("Player's ReferenceHub cannot be null!");
                referenceHub = value;
                GameObject = value.gameObject;
                Ammo = value.ammoBox;
                Inventory = value.inventory;
                GrenadeManager = value.GetComponent<GrenadeManager>();
            }
        }

        #endregion

        #region 静态缓存

        /// <summary>
        /// 存储所有已知玩家的字典，以 <see cref="GameObject"/> 为键。
        /// </summary>
        public static Dictionary<GameObject, Player> Dictionary { get; } = new Dictionary<GameObject, Player>();

        /// <summary>
        /// 获取服务器上所有玩家的只读集合。
        /// 注意：若无法获取玩家，请确保通过 ReferenceHub 正确注册。
        /// </summary>
        public static IEnumerable<Player> List => Dictionary.Values;

        /// <summary>
        /// 以玩家临时 Id（每局重置）为键的缓存字典。
        /// </summary>
        public static Dictionary<int, Player> IdsCache { get; } = new Dictionary<int, Player>();

        #endregion

        #region 基础组件

        /// <summary>
        /// 获取玩家的 <see cref="GameObject"/>。
        /// </summary>
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// 获取玩家的弹药箱组件。
        /// </summary>
        public AmmoBox Ammo { get; private set; }

        /// <summary>
        /// 获取玩家的物品栏组件。
        /// </summary>
        public Inventory Inventory { get; private set; }

        /// <summary>
        /// 获取玩家的手雷管理器组件。
        /// </summary>
        public GrenadeManager GrenadeManager { get; private set; }

        #endregion

        #region 网络
        /// <summary>
        /// 获取玩家是否为Host
        /// </summary>
        public bool IsHost => ReferenceHub.characterClassManager.IsHost;

        /// <summary>
        /// 获取玩家的网络连接对象（仅适用于 SCP-079 角色）。
        /// </summary>
        public NetworkConnection Connection => ReferenceHub.scp079PlayerScript.connectionToClient;

        /// <summary>
        /// 获取玩家的命令Sender类。
        /// </summary>
        public CommandSender CommandSender => ReferenceHub.queryProcessor._sender;

        /// <summary>
        /// 获取或设置玩家的临时唯一 ID（每局游戏重置）。
        /// </summary>
        public int Id
        {
            get => ReferenceHub.queryProcessor.NetworkPlayerId;
            set => ReferenceHub.queryProcessor.NetworkPlayerId = value;
        }

        /// <summary>
        /// 获取或设置玩家的昵称。
        /// </summary>
        public string Nickname
        {
            get => ReferenceHub.nicknameSync.Network_myNickSync;
            set => ReferenceHub.nicknameSync.Network_myNickSync = value;
        }

        /// <summary>
        /// 获取或设置玩家的 IP 地址（仅供调试或日志记录，不应用于权限判断）。
        /// </summary>
        public string IPAddress
        {
            get => ReferenceHub.queryProcessor._ipAddress;
            set => ReferenceHub.queryProcessor._ipAddress = value;
        }

        #endregion

        #region 角色
        /// <summary>
        /// 获取或设置玩家当前的角色类型。
        /// 设置时会调用 <see cref="SetRole(RoleType)"/> 方法。
        /// </summary>
        public RoleType Role
        {
            get => ReferenceHub.characterClassManager.NetworkCurClass;
            set => SetRole(value);
        }

        /// <summary>
        /// 获取玩家当前所属的队伍。
        /// </summary>
        public Team Team => Role.GetTeam();

        /// <summary>
        /// 获取玩家当前所属的阵营（如 Chaos、Facility、SCP 等）。
        /// </summary>
        public Side Side => Team.GetSide();

        /// <summary>
        /// 获取玩家当前角色对应的颜色（用于 UI 显示等）。
        /// </summary>
        public Color RoleColor => Role.GetColor();

        /// <summary>
        /// 判断玩家是否为 NTF（九尾狐）成员。
        /// </summary>
        public bool IsNTF => Team == Team.MTF;

        /// <summary>
        /// 判断玩家是否为 SCP。
        /// </summary>
        public bool IsSCP => Team == Team.SCP;

        /// <summary>
        /// 设置玩家的角色。
        /// </summary>
        /// <param name="newRole">角色类型</param>
        /// <param name="lite">是否保留位置和物品</param>
        /// <param name="isEscaped">玩家是否撤离</param>
        public void SetRole(RoleType newRole, bool lite = false, bool isEscaped = false)
        {
            ReferenceHub.characterClassManager.SetPlayersClass(newRole, GameObject, lite, isEscaped);
        }
        #endregion

        #region 交互
        /// <summary>
        /// 判断玩家是否处于被铐状态。
        /// </summary>
        public bool IsCuffed => CufferId != -1;

        /// <summary>
        /// 获取或设置铐住此玩家的玩家 ID（-1 表示未被铐）。
        /// </summary>
        public int CufferId
        {
            get => ReferenceHub.handcuffs.NetworkCufferId;
            set => ReferenceHub.handcuffs.NetworkCufferId = value;
        }

        /// <summary>
        /// 判断玩家是否正在换弹。
        /// </summary>
        public bool IsReloading => ReferenceHub.weaponManager.IsReloading();

        /// <summary>
        /// 判断玩家是否正在瞄准。
        /// </summary>
        public bool IsZooming => ReferenceHub.weaponManager.ZoomInProgress();

        /// <summary>
        /// 判断玩家是否存活。
        /// </summary>
        public bool IsAlive => !IsDead;

        /// <summary>
        /// 判断玩家是否死亡。
        /// </summary>
        public bool IsDead => Team == Team.RIP;

        #endregion

        #region 旋转、缩放、位置
        /// <summary>
        /// 获取或设置玩家的世界坐标位置。
        /// </summary>
        public Vector3 Position
        {
            get => ReferenceHub.plyMovementSync.GetRealPosition();
            set => ReferenceHub.plyMovementSync.OverridePosition(value, ReferenceHub.transform.rotation.eulerAngles.y);
        }

        /// <summary>
        /// 获取或设置玩家的旋转角度（水平和垂直）。
        /// </summary>
        public Vector2 Rotations
        {
            get => ReferenceHub.plyMovementSync.NetworkRotations;
            set => ReferenceHub.plyMovementSync.NetworkRotations = value;
        }

        /// <summary>
        /// 获取或设置玩家模型的缩放比例。
        /// </summary>
        public Vector3 Scale
        {
            get => ReferenceHub.transform.localScale;
            set
            {
                try
                {
                    ReferenceHub.transform.localScale = value;
                    foreach (Player target in List)
                        Server.SendSpawnMessage?.Invoke(null, new object[] { ReferenceHub.characterClassManager.netIdentity, target.Connection });
                }
                catch (Exception exception)
                {
                    Log.Error($"SetScale error: {exception}");
                }
            }
        }
        #endregion

        #region 管理员
        /// <summary>
        /// 向这名玩家发送管理员面板消息。
        /// </summary>
        public void RemoteAdminMessage(string message, bool success = true, string pluginName = null)
        {
            ReferenceHub.queryProcessor._sender.RaReply((pluginName ?? Assembly.GetCallingAssembly().GetName().Name) + "#" + message, success, true, string.Empty);
        }

        /// <summary>
        /// 闪烁这名玩家的标徽
        /// </summary>
        public IEnumerator<float> BlinkTag()
        {
            yield return Timing.WaitForOneFrame;

            BadgeHidden = !BadgeHidden;

            yield return Timing.WaitForOneFrame;

            BadgeHidden = !BadgeHidden;
        }

        /// <summary>
        /// 封禁这名玩家
        /// </summary>
        public void Ban(int duration, string reason, string issuer = "Console") => Server.BanPlayer.BanUser(GameObject, duration, reason, issuer, false);

        /// <summary>
        /// 踢出这名玩家
        /// </summary>
        public void Kick(string reason, string issuer = "Console") => Ban(0, reason, issuer);

        /// <summary>
        /// 获取或设置玩家的标徽隐藏
        /// </summary>
        public bool BadgeHidden
        {
            get => string.IsNullOrEmpty(ReferenceHub.serverRoles.HiddenBadge);
            set
            {
                if (value)
                    ReferenceHub.characterClassManager.CmdRequestHideTag();
                else
                    ReferenceHub.characterClassManager.CallCmdRequestShowTag(false);
            }
        }

        /// <summary>
        /// 获取或设置玩家的权限组。
        /// </summary>
        public UserGroup Group
        {
            get => ReferenceHub.serverRoles.Group;
            set => ReferenceHub.serverRoles.SetGroup(value, false, false, value.Cover);
        }

        /// <summary>
        /// 获取或设置玩家是否启用了监管者模式。
        /// </summary>
        public bool IsOverwatchEnabled
        {
            get => ReferenceHub.serverRoles.OverwatchEnabled;
            set => ReferenceHub.serverRoles.SetOverwatchStatus(value);
        }

        /// <summary>
        /// 获取或设置玩家是否启用了穿墙模式。
        /// </summary>
        public bool NoClipEnabled
        {
            get => ReferenceHub.serverRoles.NoclipReady;
            set => ReferenceHub.serverRoles.NoclipReady = value;
        }

        /// <summary>
        /// 获取或设置玩家是否启用了上帝模式。
        /// </summary>
        public bool IsGodModeEnabled
        {
            get => ReferenceHub.characterClassManager.GodMode;
            set => ReferenceHub.characterClassManager.GodMode = value;
        }

        /// <summary>
        /// 获取或设置玩家是否启用了无视权限模式。
        /// </summary>
        public bool IsBypassModeEnabled
        {
            get => ReferenceHub.serverRoles.BypassMode;
            set => ReferenceHub.serverRoles.BypassMode = value;
        }

        /// <summary>
        /// 获取或设置玩家是否被禁言。
        /// </summary>
        public bool IsMuted
        {
            get => ReferenceHub.characterClassManager.NetworkMuted;
            set => ReferenceHub.characterClassManager.NetworkMuted = value;
        }

        /// <summary>
        /// 获取或设置玩家是否被禁用对讲机或广播。
        /// </summary>
        public bool IsIntercomMuted
        {
            get => ReferenceHub.characterClassManager.NetworkIntercomMuted;
            set => ReferenceHub.characterClassManager.NetworkIntercomMuted = value;
        }
        #endregion

        #region 生命值
        /// <summary>
        /// 给这名玩家造成指定类型的伤害
        /// </summary>
        public void Hurt(float damage, DamageTypes.DamageType damageType = default, string attackerName = "Northwood", int attackerId = 0)
        {
            ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(damage, attackerName, damageType ?? DamageTypes.None, attackerId), GameObject);
        }

        /// <summary>
        /// 给指定玩家造成指定类型的伤害
        /// </summary>
        public void Hurt(float damage, Player attacker, DamageTypes.DamageType damageType = default) => Hurt(damage, damageType, attacker?.Nickname, attacker?.Id ?? 0);

        /// <summary>
        /// 杀死这名玩家。
        /// </summary>
        public void Kill(DamageTypes.DamageType damageType = default) => Hurt(-1f, damageType);

        /// <summary>
        /// 获取或设置玩家的当前生命值。
        /// </summary>
        public float Health
        {
            get => ReferenceHub.playerStats.health;
            set
            {
                ReferenceHub.playerStats.health = value;
                if (value > MaxHealth)
                    MaxHealth = (int)value;
            }
        }

        /// <summary>
        /// 获取或设置玩家的最大生命值。
        /// </summary>
        public int MaxHealth
        {
            get => ReferenceHub.playerStats.maxHP;
            set => ReferenceHub.playerStats.maxHP = value;
        }

        /// <summary>
        /// 获取或设置玩家的当前临时生命值。
        /// </summary>
        public float AdrenalineHealth
        {
            get => ReferenceHub.playerStats.unsyncedArtificialHealth;
            set
            {
                ReferenceHub.playerStats.unsyncedArtificialHealth = value;
                if (value > MaxAdrenalineHealth)
                    MaxAdrenalineHealth = (int)value;
            }
        }

        /// <summary>
        /// 获取或设置玩家的最大临时生命值。
        /// </summary>
        public int MaxAdrenalineHealth
        {
            get => ReferenceHub.playerStats.maxArtificialHealth;
            set => ReferenceHub.playerStats.maxArtificialHealth = value;
        }

        #endregion

        #region 物品
        /// <summary>
        /// 向玩家添加默认状态的物品。
        /// </summary>
        public void AddItem(ItemType itemType) => Inventory.AddNewItem(itemType);

        /// <summary>
        /// 向玩家添加物品。
        /// </summary>
        public void AddItem(Inventory.SyncItemInfo item) => Inventory.AddNewItem(item.id, item.durability, item.modSight, item.modBarrel, item.modOther);

        /// <summary>
        /// 将玩家的物品栏重置为提供的物品列表，清空已有的所有物品。
        /// </summary>
        public void ResetInventory(List<ItemType> newItems)
        {
            ClearInventory();

            if (newItems.Count > 0)
            {
                foreach (ItemType item in newItems)
                    AddItem(item);
            }
        }

        /// <summary>
        /// 将玩家的物品栏重置为提供的物品列表，清空已有的所有物品。
        /// </summary>
        public void ResetInventory(List<Inventory.SyncItemInfo> newItems) => ResetInventory(newItems.Select(item => item.id).ToList());

        /// <summary>
        /// 清空背包。
        /// </summary>
        public void ClearInventory() => Inventory.items.Clear();
        /// <summary>
        /// 获取或设置玩家当前手持的物品。
        /// </summary>
        public Inventory.SyncItemInfo CurrentItem
        {
            get => Inventory.GetItemInHand();
            set => Inventory.SetCurItem(value.id);
        }

        /// <summary>
        /// 获取玩家当前手持物品在物品栏中的索引。
        /// </summary>
        public int CurrentItemIndex => Inventory.GetItemIndex();

        public void DropItem(Inventory.SyncItemInfo item)
        {
            Inventory.SetPickup(item.id, item.durability, Position, Inventory.camera.transform.rotation, item.modSight, item.modBarrel, item.modOther);
            Inventory.items.Remove(item);
        }

        /// <summary>
        /// 从玩家的背包内移除指定物品。
        /// </summary>
        public void RemoveItem(Inventory.SyncItemInfo item) => Inventory.items.Remove(item);

        /// <summary>
        /// 从玩家的背包内移除手持的物品。
        /// </summary>
        public void RemoveItem() => Inventory.items.Remove(ReferenceHub.inventory.GetItemInHand());

        /// <summary>
        /// 设置玩家的子弹
        /// </summary>
        public void SetAmmo(AmmoType ammoType, int amount) => ReferenceHub.ammoBox.SetOneAmount((int)ammoType, amount.ToString());

        /// <summary>
        /// 获取玩家的子弹
        /// </summary>
        public int GetAmmo(AmmoType ammoType) => ReferenceHub.ammoBox.GetAmmo((int)ammoType);

        #endregion

        #region SCP-079

        /// <summary>
        /// 获取或设置 SCP-079 的可用能力列表。
        /// </summary>
        public Scp079PlayerScript.Ability079[] Abilities
        {
            get => ReferenceHub.scp079PlayerScript?.abilities;
            set
            {
                if (ReferenceHub.scp079PlayerScript != null)
                    ReferenceHub.scp079PlayerScript.abilities = value;
            }
        }

        /// <summary>
        /// 获取或设置 SCP-079 的等级配置。
        /// </summary>
        public Scp079PlayerScript.Level079[] Levels
        {
            get => ReferenceHub.scp079PlayerScript?.levels;
            set
            {
                if (ReferenceHub.scp079PlayerScript != null)
                    ReferenceHub.scp079PlayerScript.levels = value;
            }
        }

        /// <summary>
        /// 获取或设置 SCP-079 已锁定的门列表。
        /// </summary>
        public SyncListString LockedDoors
        {
            get => ReferenceHub.scp079PlayerScript?.lockedDoors;
            set
            {
                if (ReferenceHub.scp079PlayerScript != null)
                    ReferenceHub.scp079PlayerScript.lockedDoors = value;
            }
        }

        /// <summary>
        /// 获取或设置 SCP-079 的当前经验值。
        /// </summary>
        public float Experience
        {
            get => ReferenceHub.scp079PlayerScript != null ? ReferenceHub.scp079PlayerScript.Exp : float.NaN;
            set
            {
                if (ReferenceHub.scp079PlayerScript == null) return;
                ReferenceHub.scp079PlayerScript.Exp = value;
                ReferenceHub.scp079PlayerScript.OnExpChange();
            }
        }

        /// <summary>
        /// 获取或设置 SCP-079 当前激活的扬声器。
        /// </summary>
        public string Speaker
        {
            get => ReferenceHub.scp079PlayerScript?.Speaker;
            set
            {
                if (ReferenceHub.scp079PlayerScript != null)
                    ReferenceHub.scp079PlayerScript.Speaker = value;
            }
        }

        /// <summary>
        /// 获取或设置 SCP-079 当前的等级。
        /// </summary>
        public int Level
        {
            get => ReferenceHub.scp079PlayerScript != null ? ReferenceHub.scp079PlayerScript.Lvl : int.MinValue;
            set
            {
                if (ReferenceHub.scp079PlayerScript == null || ReferenceHub.scp079PlayerScript.Lvl == value)
                    return;

                ReferenceHub.scp079PlayerScript.Lvl = value;

                ReferenceHub.scp079PlayerScript.TargetLevelChanged(Connection, value);
            }
        }

        /// <summary>
        /// 获取或设置 SCP-079 当前的最大电量。
        /// </summary>
        public float MaxEnergy
        {
            get => ReferenceHub.scp079PlayerScript != null ? ReferenceHub.scp079PlayerScript.NetworkmaxMana : float.NaN;
            set
            {
                if (ReferenceHub.scp079PlayerScript == null)
                    return;

                ReferenceHub.scp079PlayerScript.NetworkmaxMana = value;
                ReferenceHub.scp079PlayerScript.levels[Level].maxMana = value;
            }
        }

        /// <summary>
        /// 获取或设置 SCP-079 当前的电量。
        /// </summary>
        public float Energy
        {
            get => ReferenceHub.scp079PlayerScript != null ? ReferenceHub.scp079PlayerScript.Mana : float.NaN;
            set
            {
                if (ReferenceHub.scp079PlayerScript == null)
                    return;

                ReferenceHub.scp079PlayerScript.Mana = value;
            }
        }
        #endregion

        #region 自定义行为

        /// <summary>
        /// 获取或设置此玩家是否启用友军伤害。
        /// </summary>
        public bool IsFriendlyFireEnabled { get; set; }

        #endregion

        #region 全局
        /// <summary>
        /// 向这名玩家发送Broadcast文本消息。
        /// </summary>
        public void Broadcast(ushort duration, string message, bool monop = false)
        {
            Server.Broadcast.TargetAddElement(Connection, message, duration, monop);
        }

        /// <summary>
        /// 清空这名玩家的Broadcast文本消息
        /// </summary>
        public void ClearBroadcasts() => Server.Broadcast.TargetClearElements(Connection);

        /// <summary>
        /// 向玩家的控制台发送消息。
        /// </summary>
        public void SendConsoleMessage(string message, string color) => SendConsoleMessage(this, message, color);

        /// <summary>
        /// 向指定玩家发送控制台消息
        /// </summary>
        public void SendConsoleMessage(Player target, string message, string color) => ReferenceHub.characterClassManager.TargetConsolePrint(target.Connection, message, color);

        /// <summary>
        /// 断开这位玩家的服务器连接
        /// </summary>
        public void Disconnect(string reason = null) => ServerConsole.Disconnect(GameObject, string.IsNullOrEmpty(reason) ? string.Empty : reason);

        /// <summary>
        /// 获取玩家所在的房间。
        /// </summary>
        public Room CurrentRoom
        {
            get
            {
                Vector3 end = Position - new Vector3(0f, 10f, 0f);
                bool flag = Physics.Linecast(Position, end, out RaycastHit raycastHit, -84058629);

                if (!flag || raycastHit.transform == null)
                    return null;

                Transform latestParent = raycastHit.transform;
                while (latestParent.parent?.parent != null)
                    latestParent = latestParent.parent;

                foreach (Room room in Map.Rooms)
                {
                    if (room.Transform == latestParent)
                        return room;
                }

                return new Room(latestParent.name, latestParent, latestParent.position);
            }
        }
        #endregion

        #region 玩家获取
        /// <summary>
        /// 通过昵称（模糊匹配）查找玩家。
        /// </summary>
        /// <param name="nickname">要查找的昵称（支持部分匹配）</param>
        /// <returns>最匹配的 Player，未找到则返回 null</returns>
        public static Player Get(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
                return null;

            Player bestMatch = null;
            int maxNameLength = 31;
            int minDistance = 31;
            string inputLower = nickname.ToLower();

            foreach (Player player in List)
            {
                if (!player.Nickname.Contains(nickname, StringComparison.OrdinalIgnoreCase))
                    continue;

                string paddedInput = inputLower;
                string paddedNick = player.Nickname.ToLower();

                if (paddedInput.Length < maxNameLength)
                    paddedInput = paddedInput.PadRight(maxNameLength, 'z');
                if (paddedNick.Length < maxNameLength)
                    paddedNick = paddedNick.PadRight(maxNameLength, 'z');

                int distance = paddedInput.GetDistance(paddedNick);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestMatch = player;
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// 通过<see cref="global::ReferenceHub"/>获取玩家类。
        /// </summary>
        /// <returns></returns>
        public static Player Get(ReferenceHub referenceHub) => referenceHub == null ? null : Get(referenceHub.gameObject);

        /// <summary>
        /// 通过<see cref="UnityEngine.GameObject"/>获取玩家。
        /// </summary>
        /// <param name="gameObject">游戏对象</param>
        /// <returns></returns>
        public static Player Get(GameObject gameObject)
        {
            if (gameObject == null)
                return null;

            Dictionary.TryGetValue(gameObject, out Player player);

            return player;
        }

        /// <summary>
        /// 通过临时ID获取玩家。
        /// </summary>
        /// <param name="id">临时玩家ID</param>
        /// <returns></returns>
        public static Player Get(int id)
        {
            if (IdsCache.TryGetValue(id, out Player player) && player?.ReferenceHub != null)
                return player;

            foreach (Player playerFound in Dictionary.Values)
            {
                if (playerFound.Id != id)
                    continue;

                IdsCache[id] = playerFound;

                return playerFound;
            }

            return null;
        }
        #endregion
    }
}