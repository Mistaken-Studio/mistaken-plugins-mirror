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
            {
                if (UnityEngine.Random.Range(1, 101) < 25)
                    Exiled.API.Features.Cassie.Message(Handler.CIAnnouncments[UnityEngine.Random.Range(0, Handler.CIAnnouncments.Length)]);
                return false;
            }
            else
                return true;
        }
    }
}
