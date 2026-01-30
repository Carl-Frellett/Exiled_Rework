using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RExiled.API.Features
{
    public static class Map
    {
        #region 缓存字段

        private static readonly List<Room> RoomsValue = new List<Room>(250);
        private static readonly List<Door> DoorsValue = new List<Door>(250);
        private static readonly List<Lift> LiftsValue = new List<Lift>(10);
        private static readonly List<TeslaGate> TeslasValue = new List<TeslaGate>(10);

        private static readonly ReadOnlyCollection<Room> ReadOnlyRoomsValue = RoomsValue.AsReadOnly();
        private static readonly ReadOnlyCollection<Door> ReadOnlyDoorsValue = DoorsValue.AsReadOnly();
        private static readonly ReadOnlyCollection<Lift> ReadOnlyLiftsValue = LiftsValue.AsReadOnly();
        private static readonly ReadOnlyCollection<TeslaGate> ReadOnlyTeslasValue = TeslasValue.AsReadOnly();

        private static Broadcast _broadcast;
        private static DecontaminationLCZ _decontaminationLCZ;

        #endregion

        #region 属性

        /// <summary>
        /// 获取所有房间的只读集合。
        /// </summary>
        public static ReadOnlyCollection<Room> Rooms
        {
            get
            {
                if (RoomsValue.Count == 0)
                {
                    var roomTransforms = Object.FindObjectsOfType<Transform>()
                        .Where(t => t.CompareTag("Room"))
                        .ToList();

                    RoomsValue.AddRange(roomTransforms.Select(t => new Room(t.name, t, t.position)));

                    const string surfaceRoomName = "Root_*&*Outside Cams";
                    var surfaceObj = GameObject.Find(surfaceRoomName);
                    if (surfaceObj != null)
                    {
                        var surfaceTransform = surfaceObj.transform;
                        RoomsValue.Add(new Room(surfaceRoomName, surfaceTransform, surfaceTransform.position));
                    }
                }
                return ReadOnlyRoomsValue;
            }
        }

        /// <summary>
        /// 获取所有门的只读集合。
        /// </summary>
        public static ReadOnlyCollection<Door> Doors
        {
            get
            {
                if (DoorsValue.Count == 0)
                    DoorsValue.AddRange(Object.FindObjectsOfType<Door>());
                return ReadOnlyDoorsValue;
            }
        }

        /// <summary>
        /// 获取所有电梯的只读集合。
        /// </summary>
        public static ReadOnlyCollection<Lift> Lifts
        {
            get
            {
                if (LiftsValue.Count == 0)
                    LiftsValue.AddRange(Object.FindObjectsOfType<Lift>());
                return ReadOnlyLiftsValue;
            }
        }

        /// <summary>
        /// 获取所有特斯拉门的只读集合。
        /// </summary>
        public static ReadOnlyCollection<TeslaGate> TeslaGates
        {
            get
            {
                if (TeslasValue.Count == 0)
                    TeslasValue.AddRange(Object.FindObjectsOfType<TeslaGate>());
                return ReadOnlyTeslasValue;
            }
        }

        /// <summary>
        /// 获取本地玩家的 DecontaminationLCZ 组件。
        /// </summary>
        internal static DecontaminationLCZ DecontaminationLCZ
        {
            get
            {
                if (_decontaminationLCZ == null)
                    _decontaminationLCZ = PlayerManager.localPlayer.GetComponent<DecontaminationLCZ>();
                return _decontaminationLCZ;
            }
        }

        /// <summary>
        /// 获取本地玩家的 Broadcast 组件（用于全局广播）。
        /// </summary>
        private static Broadcast BroadcastComponent
        {
            get
            {
                if (_broadcast == null)
                    _broadcast = PlayerManager.localPlayer.GetComponent<Broadcast>();
                return _broadcast;
            }
        }

        #endregion

        #region 全局状态属性

        /// <summary>
        /// 获取 LCZ 是否已完成净化。
        /// </summary>
        public static bool IsLCZDecontaminated => DecontaminationLCZ.GetCurAnnouncement() > 5;

        /// <summary>
        /// 获取已激活的发电机数量。
        /// </summary>
        public static int ActivatedGenerators => Generator079.mainGenerator.totalVoltage;

        /// <summary>
        /// 获取所有 SCP-079 摄像头。
        /// </summary>
        public static Camera079[] Cameras => Scp079PlayerScript.allCameras;

        #endregion

        #region 全局操作方法

        /// <summary>
        /// 向所有玩家广播消息。
        /// 使用本地玩家的 Broadcast 组件。
        /// </summary>
        /// <param name="duration">持续时间（秒）</param>
        /// <param name="message">消息内容</param>
        /// <param name="monospaced">是否使用等宽字体</param>
        public static void Broadcast(ushort duration, string message, bool monospaced = false)
        {
            BroadcastComponent.RpcAddElement(message, duration, monospaced);
        }

        /// <summary>
        /// 清除所有玩家的广播。
        /// </summary>
        public static void ClearBroadcasts() => BroadcastComponent.RpcClearElements();

        /// <summary>
        /// 开始 LCZ 净化流程。
        /// </summary>
        /// <param name="isAnnouncementGlobal">是否全局播放公告</param>
        public static void StartDecontamination(bool isAnnouncementGlobal = true) =>
            DecontaminationLCZ.RpcPlayAnnouncement(5, isAnnouncementGlobal);

        /// <summary>
        /// 关闭设施内所有灯光（入口区除外）。
        /// </summary>
        /// <param name="duration">持续时间</param>
        /// <param name="onlyHeavy">是否仅关闭重型灯</param>
        public static void TurnOffAllLights(float duration, bool onlyHeavy = false) =>
            Generator079.generators[0].RpcCustomOverchargeForOurBeautifulModCreators(duration, onlyHeavy);

        #endregion

        #region 扩展方法

        /// <summary>
        /// 获取指定角色类型的随机出生点。
        /// 与 EXILED 一致，通过 SpawnpointManager 获取。
        /// </summary>
        /// <param name="roleType">角色类型</param>
        /// <returns>出生点位置，若未找到则返回 Vector3.zero</returns>
        public static Vector3 GetRandomSpawnPoint(this RoleType roleType)
        {
            GameObject randomPosition = Object.FindObjectOfType<SpawnpointManager>().GetRandomPosition(roleType);
            return randomPosition == null ? Vector3.zero : randomPosition.transform.position;
        }

        #endregion

        #region 缓存管理

        /// <summary>
        /// 清除所有缓存的地图对象（用于回合重置后刷新）。
        /// </summary>
        internal static void ClearCache()
        {
            RoomsValue.Clear();
            DoorsValue.Clear();
            LiftsValue.Clear();
            TeslasValue.Clear();
            _broadcast = null;
            _decontaminationLCZ = null;
        }

        #endregion
    }
}