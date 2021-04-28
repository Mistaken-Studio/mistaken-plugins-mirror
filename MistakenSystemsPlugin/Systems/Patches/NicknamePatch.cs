﻿using HarmonyLib;
using System.Collections.Generic;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(NicknameSync), "UpdateNickname", typeof(string))]
    [HarmonyPatch(typeof(NicknameSync), "CallCmdSetNick", typeof(string))]
    public static class NicknamePatch
    {
        public static readonly Dictionary<string, string> RealNicknames = new Dictionary<string, string>();
        public static bool Prefix(NicknameSync __instance, ref string n)
        {
            RealNicknames[__instance._hub.characterClassManager.UserId] = n;
            if (CommandsExtender.Commands.FakeNickCommand.FullNicknames.TryGetValue(__instance._hub.characterClassManager.UserId, out string newNick))
                n = newNick;
            return true;
        }
    }
    [HarmonyPatch(typeof(NicknameSync), "SetNick", typeof(string))]
    static class NicknamePatch2
    {
        public static bool Prefix(NicknameSync __instance, ref string nick)
        {
            NicknamePatch.RealNicknames[__instance._hub.characterClassManager.UserId] = nick;
            if (CommandsExtender.Commands.FakeNickCommand.FullNicknames.TryGetValue(__instance._hub.characterClassManager.UserId, out string newNick))
                nick = newNick;
            return true;
        }
    }
}
