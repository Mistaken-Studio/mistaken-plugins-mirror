using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Respawning;

namespace Gamer.Mistaken.BetterRP
{
    [HarmonyPatch(typeof(RespawnEffectsController), "ServerExecuteEffects")]
    public static class RespawnManagerPatch
    {
        public static bool Prefix(RespawnEffectsController.EffectType type, SpawnableTeamType team)
        {
            if (PluginHandler.Config.IsRP() && type == RespawnEffectsController.EffectType.UponRespawn && team == SpawnableTeamType.ChaosInsurgency)
                return false;
            else
                return true;
        }
    }
}
