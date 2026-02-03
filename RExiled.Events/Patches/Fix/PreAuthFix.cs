using GameCore;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using Mirror.LiteNetLib4Mirror;
using System;
using System.Collections.Generic;

namespace RExiled.Events.Patches.Fix
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport), nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest), typeof(ConnectionRequest))]
    public class PreAuthFix
    {
        private static readonly Random OfflineIdRandom = new Random();

        public static bool Prefix(ref ConnectionRequest request)
        {
            try
            {
                HandleConnection(request);
                return false;
            }
            catch (Exception exception)
            {
                RExiled.API.Features.Log.Error($"PreAuthFix error: {exception}");
                return true;
            }
        }

        private static void HandleConnection(ConnectionRequest request)
        {
            NetDataWriter rejectData = new NetDataWriter();
            try
            {
                byte result1;
                byte result2;
                int position = request.Data.Position;

                if (!request.Data.TryGetByte(out result1) || !request.Data.TryGetByte(out result2) ||
                    result1 != CustomNetworkManager.Major || result2 != CustomNetworkManager.Minor)
                {
                    rejectData.Reset();
                    rejectData.Put(3);
                    request.RejectForce(rejectData);
                }
                else
                {
                    if (CustomLiteNetLib4MirrorTransport.IpRateLimiting)
                    {
                        if (CustomLiteNetLib4MirrorTransport.IpRateLimit.Contains(request.RemoteEndPoint.Address.ToString()))
                        {
                            ServerConsole.AddLog(string.Format("Incoming connection from endpoint {0} rejected due to exceeding the rate limit.", request.RemoteEndPoint));
                            ServerLogs.AddLog(ServerLogs.Modules.Networking,
                                string.Format("Incoming connection from endpoint {0} rejected due to exceeding the rate limit.", request.RemoteEndPoint),
                                ServerLogs.ServerLogType.RateLimit);
                            rejectData.Reset();
                            rejectData.Put(12);
                            request.RejectForce(rejectData);
                            return;
                        }
                        CustomLiteNetLib4MirrorTransport.IpRateLimit.Add(request.RemoteEndPoint.Address.ToString());
                    }

                    string offlineId = GenerateOfflineId();

                    if (!CharacterClassManager.OnlineMode)
                    {
                        KeyValuePair<BanDetails, BanDetails> keyValuePair = BanHandler.QueryBan(null, request.RemoteEndPoint.Address.ToString());
                        if (keyValuePair.Value != null)
                        {
                            ServerConsole.AddLog(string.Format("Player tried to connect from banned endpoint {0}.", request.RemoteEndPoint));
                            rejectData.Reset();
                            rejectData.Put(6);
                            rejectData.Put(keyValuePair.Value.Expires);
                            rejectData.Put(keyValuePair.Value?.Reason ?? string.Empty);
                            request.RejectForce(rejectData);
                        }
                        else
                        {
                            request.Accept();
                        }
                    }
                    else
                    {
                        ulong result4 = TimeBehaviour.CurrentUnixTimestamp + 3600UL;
                        byte result5 = 0;
                        byte[] result7 = new byte[0];

                        CentralAuthPreauthFlags flags = (CentralAuthPreauthFlags)result5;


                        if (CustomLiteNetLib4MirrorTransport.UserRateLimiting)
                        {
                            if (CustomLiteNetLib4MirrorTransport.UserRateLimit.Contains(offlineId))
                            {
                                ServerConsole.AddLog(string.Format("Incoming connection from {0} ({1}) rejected due to exceeding the rate limit.", offlineId, request.RemoteEndPoint));
                                ServerLogs.AddLog(ServerLogs.Modules.Networking,
                                    string.Format("Incoming connection from endpoint {0} ({1}) rejected due to exceeding the rate limit.", offlineId, request.RemoteEndPoint),
                                    ServerLogs.ServerLogType.RateLimit);
                                rejectData.Reset();
                                rejectData.Put(12);
                                request.RejectForce(rejectData);
                                return;
                            }
                            CustomLiteNetLib4MirrorTransport.UserRateLimit.Add(offlineId);
                        }

                        if (!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreBans) || !ServerStatic.GetPermissionsHandler().IsVerified)
                        {
                            KeyValuePair<BanDetails, BanDetails> keyValuePair = BanHandler.QueryBan(offlineId, request.RemoteEndPoint.Address.ToString());
                            if (keyValuePair.Key != null || keyValuePair.Value != null)
                            {
                                ServerConsole.AddLog(string.Format("{0} {1} tried to connect from {2} endpoint {3}.",
                                    keyValuePair.Key == null ? "Player" : "Banned player", offlineId,
                                    keyValuePair.Value == null ? "" : "banned ", request.RemoteEndPoint));
                                ServerLogs.AddLog(ServerLogs.Modules.Networking,
                                    string.Format("{0} {1} tried to connect from {2} endpoint {3}.",
                                    keyValuePair.Key == null ? "Player" : "Banned player", offlineId,
                                    keyValuePair.Value == null ? "" : "banned ", request.RemoteEndPoint),
                                    ServerLogs.ServerLogType.ConnectionUpdate);
                                rejectData.Reset();
                                rejectData.Put(6);
                                NetDataWriter netDataWriter1 = rejectData;
                                BanDetails key = keyValuePair.Key;
                                netDataWriter1.Put(key != null ? key.Expires : keyValuePair.Value.Expires);
                                NetDataWriter netDataWriter2 = rejectData;
                                string str = keyValuePair.Key?.Reason ?? keyValuePair.Value?.Reason ?? string.Empty;
                                netDataWriter2.Put(str);
                                request.Reject(rejectData);
                                return;
                            }
                        }

                        if ((!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreWhitelist) || !ServerStatic.GetPermissionsHandler().IsVerified) &&
                            !WhiteList.IsWhitelisted(offlineId))
                        {
                            ServerConsole.AddLog(string.Format("Player {0} tried joined from endpoint {1}, but is not whitelisted.", offlineId, request.RemoteEndPoint));
                            rejectData.Reset();
                            rejectData.Put(7);
                            request.Reject(rejectData);
                        }
                        else if (CustomLiteNetLib4MirrorTransport.Geoblocking != GeoblockingMode.None &&
                                 (!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreGeoblock) || !ServerStatic.GetPermissionsHandler().BanTeamBypassGeo) &&
                                 (!CustomLiteNetLib4MirrorTransport.GeoblockIgnoreWhitelisted || !WhiteList.IsOnWhitelist(offlineId)) &&
                                 (CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Whitelist && !CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains("CN") ||
                                  CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Blacklist && CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains("CN")))
                        {
                            ServerConsole.AddLog(string.Format("Player {0} ({1}) tried joined from blocked country CN.", offlineId, request.RemoteEndPoint));
                            rejectData.Reset();
                            rejectData.Put(9);
                            request.RejectForce(rejectData);
                        }
                        else
                        {
                            int num = CustomNetworkManager.slots;
                            if (flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot) && ServerStatic.GetPermissionsHandler().BanTeamSlots)
                                num = LiteNetLib4MirrorNetworkManager.singleton.maxConnections;
                            else if (ConfigFile.ServerConfig.GetBool("use_reserved_slots", true) && ReservedSlot.HasReservedSlot(offlineId))
                                num += CustomNetworkManager.reservedSlots;

                            if (LiteNetLib4MirrorCore.Host.PeersCount < num)
                            {
                                if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
                                    CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].SetUserId(offlineId);
                                else
                                    CustomLiteNetLib4MirrorTransport.UserIds.Add(request.RemoteEndPoint, new PreauthItem(offlineId));

                                request.Accept();
                                ServerConsole.AddLog(string.Format("Player {0} preauthenticated from endpoint {1}.", offlineId, request.RemoteEndPoint));
                                ServerLogs.AddLog(ServerLogs.Modules.Networking,
                                    string.Format("{0} preauthenticated from endpoint {1}.", offlineId, request.RemoteEndPoint),
                                    ServerLogs.ServerLogType.ConnectionUpdate);
                            }
                            else
                            {
                                rejectData.Reset();
                                rejectData.Put(1);
                                request.Reject(rejectData);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                RExiled.API.Features.Log.Error(string.Format("Player from endpoint {0} failed to preauthenticate: {1}", request.RemoteEndPoint, exception.Message));
                rejectData.Reset();
                rejectData.Put(4);
                request.RejectForce(rejectData);
            }
        }

        private static string GenerateOfflineId()
        {
            lock (OfflineIdRandom)
            {
                char first = (char)('1' + (OfflineIdRandom.Next(9)));
                char[] id = new char[17];
                id[0] = first;
                for (int i = 1; i < 17; i++)
                {
                    id[i] = (char)('0' + OfflineIdRandom.Next(10));
                }
                return new string(id);
            }
        }
    }
}