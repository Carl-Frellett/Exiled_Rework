// RExiled.API.Extensions.NetworkConnectionExtensions.cs
using Mirror;
using RExiled.API.Features;
using UnityEngine;

namespace RExiled.API.Extensions
{
    public static class NetworkConnection
    {
        public static GameObject GetPlayerObject(this Mirror.NetworkConnection conn)
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

        public static Player GetRExiledPlayer(this Mirror.NetworkConnection conn)
        {
            GameObject go = conn.GetPlayerObject();
            return go == null ? null : Player.Get(go);
        }
    }
}