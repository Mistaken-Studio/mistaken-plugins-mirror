using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.Events;
using HarmonyLib;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.CITester
{
    [HarmonyPatch(typeof(Hints.HintDisplay), "Show")]
    internal static class HintPatch
    {
        private static bool Prefix(Hints.HintDisplay __instance)
        {
            return false; 
        }
    }
}
