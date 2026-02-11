using DreamPlugin.Game.CustomRole.Extensions;
using RExiled.API.Features;
using RExiled.Events.EventArgs.Player;
using System.Collections.Generic;

namespace DreamPlugin.Game.CustomRole
{
    public abstract class CustomRole
    {
        public string Name = "CustomRole";
        public string SpawnDescription = "";
        public string DiedDescription = "";
        public SpawnConditionType SpawnCondition = SpawnConditionType.CommandOnly;
        public RoleType SpawnRoleType = RoleType.ClassD;
        public int SpawnCapacityLimit = 0;
        public int SpawnRoleCapacityLimit = 0;
        public int RoleHealth = 100;
        public int RoleMaxHealth = 100;
        public List<ItemType> RoleInventory = new List<ItemType>();
        public bool IsJoinSpawnQueue = true;

        public Player CurrentPlayer { get; private set; } = null;
        private bool _isDestroyed = false;

        public void AssignTo(Player player)
        {
            if (CurrentPlayer != null || _isDestroyed || player == null) return;

            CurrentPlayer = player;
            _isDestroyed = false;

            string original = CurrentPlayer.Nickname;
            CurrentPlayer.SetNickname($"[{Name}] {original}");

            CurrentPlayer.SetRole(SpawnRoleType, true);
            CurrentPlayer.MaxHealth = RoleMaxHealth;
            CurrentPlayer.Health = RoleHealth;
            CurrentPlayer.ResetInventory(RoleInventory);

            if (!string.IsNullOrWhiteSpace(SpawnDescription))
                BroadcastSystem.BroadcastSystem.ShowToPlayer(CurrentPlayer, $"[个人消息] {SpawnDescription}", 5f);

            OnSpawn();

            RExiled.Events.Handlers.Player.Died += OnInternalPlayerDied;
            RExiled.Events.Handlers.Player.Left += OnInternalPlayerLeft;
            RExiled.Events.Handlers.Player.ChangedRole += OnInternalPlayerChangedRole;
        }

        public void Destroy()
        {
            if (_isDestroyed || CurrentPlayer == null) return;
            _isDestroyed = true;

            string nick = CurrentPlayer.Nickname;
            string prefix = $"{Name} | ";
            if (nick.StartsWith(prefix))
                CurrentPlayer.SetNickname(nick.Substring(prefix.Length));

            OnDestroy();

            RExiled.Events.Handlers.Player.Died -= OnInternalPlayerDied;
            RExiled.Events.Handlers.Player.Left -= OnInternalPlayerLeft;
            RExiled.Events.Handlers.Player.ChangedRole -= OnInternalPlayerChangedRole;

            CurrentPlayer = null;
        }

        private void OnInternalPlayerDied(DiedEventArgs ev)
        {
            if (ev.Target == CurrentPlayer)
            {
                if (!string.IsNullOrWhiteSpace(DiedDescription.Trim()))
                    BroadcastSystem.BroadcastSystem.ShowGlobal(DiedDescription, 5f);
                Destroy();
            }
        }

        private void OnInternalPlayerLeft(LeftEventArgs ev)
        {
            if (ev.Player == CurrentPlayer)
                Destroy();
        }

        private void OnInternalPlayerChangedRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player == CurrentPlayer && !_isDestroyed)
            {
                Destroy();
            }
        }

        public abstract void OnSpawn();
        public abstract void OnDestroy();
    }
}