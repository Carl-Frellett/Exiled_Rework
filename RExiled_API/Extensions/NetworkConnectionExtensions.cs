// RExiled.API.Extensions.NetworkConnectionExtensions.cs
using Mirror;
using RExiled.API.Features;
using UnityEngine;

namespace RExiled.API.Extensions
{
    public static class NetworkConnectionExtensions
    {
        public static GameObject GetPlayerObject(this NetworkConnection conn)
        {
            try
            {
                if (conn?.identity != null && conn.identity.gameObject != null)
                {
                    GameObject go = conn.identity.gameObject;
                    return go.CompareTag("Player") ? go : null;
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }

        public static Player GetRExiledPlayer(this NetworkConnection conn)
        {
            GameObject go = conn.GetPlayerObject();
            return go == null ? null : Player.Get(go);
        }
    }
}