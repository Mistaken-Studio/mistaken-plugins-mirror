﻿using Exiled.API.Features;
using Gamer.Utilities;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.Systems.Patches
{

    [HarmonyPatch(typeof(Exiled.API.Features.Player), "ShowHint")] //Check
    internal static class HintPatch
    {
        private static readonly Dictionary<string, int> Tries = new Dictionary<string, int>();
        public static bool Prefix(Player __instance, ref string message, float duration = 3f)
        {
            if (!RealPlayers.List.Contains(__instance))
            {
                string msg = message;
                if (!Tries.ContainsKey(message))
                    Tries.Add(message, 10);
                else
                {
                    Tries[message]--;
                    if (Tries[message] == 0)
                    {
                        Tries.Remove(message);
                        return false;
                    }
                }
                Gamer.Utilities.BetterCourotines.CallDelayed(0.5f, () =>
                {
                    __instance.ShowHint(msg, duration);
                }, "HintPatch");
                return false;
            }
            Tries.Remove(message);
            message = message.Replace("\n", "<br>").Replace("\\n", "<br>").Replace("{", "[").Replace("}", "]");
            __instance.HintDisplay.Show(new Hints.TextHint(message, new Hints.HintParameter[]
            {
                new Hints.StringHintParameter(message)
            }, new Hints.HintEffect[]
            {
                new Hints.AlphaEffect(1f, 0, duration)
            }, duration));
            return false;
        }
    }
}
