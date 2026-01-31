using HarmonyLib;
using GameCore;
using UnityEngine;
using System.Text.RegularExpressions;

namespace RExiled.Events.Patches
{
    [HarmonyPatch(typeof(Console), nameof(Console.AddLog))]
    internal class RoundEndedPatch
    {
        private static readonly Regex FinishedRegex = new Regex(@"Round finished! Anomalies: \d+", RegexOptions.Compiled);

        private static void Prefix(string text, Color _, bool __)
        {
            if (RExiled.Events.Events.GetRoundDuration() < 2f) return;

            if (FinishedRegex.IsMatch(text))
            {
                RExiled.Events.Handlers.Server.OnRoundEnded();
            }
        }
    }
}