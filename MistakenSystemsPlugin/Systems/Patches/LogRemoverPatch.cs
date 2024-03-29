﻿#pragma warning disable IDE0079
#pragma warning disable IDE0060

using Exiled.API.Features;
using Gamer.RoundLoggerSystem;
using HarmonyLib;
using System;
using System.Linq;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(Log), "SendRaw")] //Check
    internal static class LogRemoverPatch
    {
        public static bool Prefix(ref object message, ConsoleColor color)
        {
            if (!Gamer.Mistaken.Utilities.APILib.API.GetAPIKey(out string key))
                return true;
            if (!Gamer.Mistaken.Utilities.APILib.API.GetSteamAPIKey(out string steamKey))
                return true;
            if (!Gamer.Mistaken.Utilities.APILib.API.GetSocketKey(out string socketKey))
                return true;
            var txt = message.ToString();
            if (txt.Contains(key))
            {
                Log.Warn("Skipped debug, contains API Key");
                return false;
            }
            if (txt.Contains(steamKey))
            {
                Log.Warn("Skipped debug, contains Steam API Key");
                return false;
            }
            if (txt.Contains(socketKey))
            {
                Log.Warn("Skipped debug, contains Socket Key");
                return false;
            }

            if (txt.Contains("[ERROR]"))
                RoundLogger.Log("LOGGER", "ERROR", string.Join(" ", txt.Split(' ').Skip(1)));
            else if (txt.Contains("[WARN]"))
                RoundLogger.Log("LOGGER", "WARN", string.Join(" ", txt.Split(' ').Skip(1)));

            return true;
        }
    }
}
