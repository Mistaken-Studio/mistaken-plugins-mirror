using Exiled.API.Extensions;
using Exiled.API.Features;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using RemoteAdmin;
using System;
using System.Collections.Generic;

namespace Gamer.Mistaken.Systems.Patches
{
    //[HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    public static class RoundRestartPatch
    {
        public static bool Prefix(PlayerStats __instance)
        {
            CustomLiteNetLib4MirrorTransport.DelayConnections = true;
            CustomLiteNetLib4MirrorTransport.UserIdFastReload.Clear();
            IdleMode.PauseIdleMode = true;
            Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer[] array = UnityEngine.Object.FindObjectsOfType<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>();
            for (int i = 0; i < array.Length; i++)
                array[i].OnDisable();
            if (CustomNetworkManager.EnableFastRestart)
            {
                foreach (ReferenceHub referenceHub in ReferenceHub.GetAllHubs().Values)
                {
                    if (referenceHub.isDedicatedServer)
                        continue;
                    var item = Player.Get(referenceHub);
                    if (Systems.Handler.PlayerPreferencesDict[referenceHub.characterClassManager.UserId].HasFlag(API.PlayerPreferences.DISABLE_FAST_ROUND_RESTART))
                        MirrorExtensions.SendFakeTargetRpc(item, referenceHub.networkIdentity, typeof(PlayerStats), nameof(PlayerStats.RpcRoundrestart), (float)PlayerPrefsSl.Get("LastRoundrestartTime", 5000) / 1000f, true);
                    else
                    {
                        try
                        {
                            CustomLiteNetLib4MirrorTransport.UserIdFastReload.Add(referenceHub.characterClassManager.UserId);
                        }
                        catch (Exception ex)
                        {
                            ServerConsole.AddLog("Exception occured during processing online player list for Fast Restart: " + ex.Message, ConsoleColor.Yellow);
                        }
                        MirrorExtensions.SendFakeTargetRpc(item, referenceHub.networkIdentity, typeof(PlayerStats), nameof(PlayerStats.RpcFastRestart));
                    }
                }
                //__instance.RpcFastRestart();
                PlayerStats.StaticChangeLevel(false);
            }
            else
            {
                if (ServerStatic.StopNextRound == ServerStatic.NextRoundAction.DoNothing)
                    __instance.RpcRoundrestart((float)PlayerPrefsSl.Get("LastRoundrestartTime", 5000) / 1000f, true);
                __instance.Invoke("ChangeLevel", 2.5f);
            }
            return false;
        }
    }
}
