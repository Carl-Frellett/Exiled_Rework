using Assets._Scripts.Dissonance;
using HarmonyLib;
using MEC;
using RExiled.API.Features;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RExiled.Events.Patches.Fix
{
    [HarmonyPatch(typeof(Intercom), nameof(Intercom.CallCmdSetTransmit))]
    public class IntercomSpeakingEvent
    {
        private static CoroutineHandle _checkCoroutine = default;

        public static bool Prefix(Intercom __instance, bool player)
        {
            try
            {
                string playerName = __instance.GetComponent<NicknameSync>()?.MyNick ?? "Unknown";

                if (Intercom.AdminSpeaking) return false;

                var ccm = __instance.GetComponent<CharacterClassManager>();
                if (ccm == null) return false;

                var curClass = ccm.Classes.SafeGet(ccm.CurClass);
                if (curClass.team == Team.SCP || curClass.team == Team.RIP) return false;

                ReferenceHub playerHub = __instance.GetComponentInParent<ReferenceHub>();
                if (playerHub == null)
                {
                    return false;
                }
                if (player && Intercom.host?.Networkspeaker != null)
                {
                    return false;
                }

                if (Intercom.host == null)
                {
                    GameObject intercomObj = GameObject.Find("Intercom");
                    Intercom.host = intercomObj?.GetComponent<Intercom>() ?? __instance;
                }

                Intercom.host.intercomSupported = true;

                if (Intercom.host.area == null)
                {
                    Transform zone = GameObject.Find("IntercomSpeakingZone")?.transform;
                    Intercom.host.area = zone ?? Intercom.host.transform;
                }

                float dist = Vector3.Distance(playerHub.transform.position, Intercom.host.area.position);
                if (dist >= Intercom.host.triggerDistance) return false;

                DissonanceUserSetup dissonance = playerHub.gameObject.GetComponentInChildren<DissonanceUserSetup>();

                if (player)
                {
                    if (dissonance != null)
                    {
                        dissonance.IntercomAsHuman = true;
                    }

                    Intercom.host.Networkspeaker = playerHub.gameObject;
                    Intercom.host.speaking = true;
                    Intercom.host._inUse = false;
                    Intercom.host.speechRemainingTime = 65535f;
                    Intercom.host.remainingCooldown = 0f;

                    Intercom.host.RpcPlaySound(true, playerHub.queryProcessor.PlayerId);

                    if (_checkCoroutine != default)
                        Timing.KillCoroutines(_checkCoroutine);
                    _checkCoroutine = Timing.RunCoroutine(CheckSpeakerPosition(playerHub));
                }
                else
                {
                    if (Intercom.host.Networkspeaker == playerHub.gameObject)
                    {
                        if (dissonance != null)
                            dissonance.IntercomAsHuman = false;

                        Intercom.host.Networkspeaker = null;
                        Intercom.host.speaking = false;
                        Intercom.host._inUse = false;
                        Intercom.host.RpcPlaySound(false, playerHub.queryProcessor.PlayerId);

                        if (_checkCoroutine != default)
                            Timing.KillCoroutines(_checkCoroutine);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"【广播调试】异常: {ex}");
                return true;
            }
        }

        private static IEnumerator<float> CheckSpeakerPosition(ReferenceHub speakerHub)
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(0.5f);

                if (Intercom.host?.Networkspeaker == null) break;

                if (Intercom.host.Networkspeaker != speakerHub.gameObject) break;

                if (speakerHub == null)
                {
                    ForceStopBroadcast(speakerHub);
                    break;
                }

                if (Intercom.host.area != null)
                {
                    float dist = Vector3.Distance(speakerHub.transform.position, Intercom.host.area.position);
                    if (dist > Intercom.host.triggerDistance)
                    {
                        ForceStopBroadcast(speakerHub);
                        break;
                    }
                }
            }
        }

        private static void ForceStopBroadcast(ReferenceHub hub)
        {
            var dissonance = hub?.gameObject.GetComponentInChildren<DissonanceUserSetup>();
            if (dissonance != null)
            {
                dissonance.IntercomAsHuman = false;
            }

            Intercom.host.Networkspeaker = null;
            Intercom.host.speaking = false;
            Intercom.host._inUse = false;
            Intercom.host.RpcPlaySound(false, 0);

            if (_checkCoroutine != default)
            {
                Timing.KillCoroutines(_checkCoroutine);
                _checkCoroutine = default;
            }
        }
    }
}