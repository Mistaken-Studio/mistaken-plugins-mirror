using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.Utilities;
using HarmonyLib;
using MEC;
using Mirror;
using NPCS;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.CITester
{
    public class PluginHandler : Plugin<Config>
    {
        public override string Author => "Gamer";
        public override string Name => "CITester";
        public override void OnDisabled()
        {
            IdleMode.PauseIdleMode = false;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= Server_WaitingForPlayers;
            base.OnDisabled();
        }

        public override void OnEnabled()
        {
            IdleMode.PauseIdleMode = true;
            Exiled.Events.Handlers.Server.WaitingForPlayers += Server_WaitingForPlayers;

            Exiled.Events.Events.DisabledPatchesHashSet.Add(typeof(PlayerPositionManager).GetMethod("TransmitData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            Exiled.Events.Events.Instance.ReloadDisabledPatches();
            var harmony = new HarmonyLib.Harmony("gamer.citester");
            harmony.PatchAll();

            base.OnEnabled();
        }

        private void Server_WaitingForPlayers()
        {
            SpawnTestObject();
            SpawnTestObject();
            Log.Debug(Player.List.Count());
            foreach (var item in Player.List)
            {
                Log.Debug(item);
            }
            Log.Debug(RealPlayers.List.Count());
            foreach (var item in RealPlayers.List)
            {
                Log.Debug(item);
            }
            Timing.CallDelayed(1f, () =>
            {
                Log.Debug(Player.List.Count());
                foreach (var item in Player.List)
                {
                    Log.Debug(item);
                }
                Log.Debug(RealPlayers.List.Count());
                foreach (var item in RealPlayers.List)
                {
                    Log.Debug(item);
                }
            });
        }

        private static int testSubjectId = 2;
        public static int SpawnTestObject()
        {
            testSubjectId++;
            Exiled.Events.Handlers.Player.OnPreAuthenticating(new Exiled.Events.EventArgs.PreAuthenticatingEventArgs("76561198134629649@steam", null, 0, 0, "PL", true));
            GameObject obj = UnityEngine.Object.Instantiate<GameObject>(NetworkManager.singleton.spawnPrefabs.FirstOrDefault((GameObject p) => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
            obj.transform.localScale = Vector3.one;
            obj.transform.position = new Vector3(0, 6000, 0);
            QueryProcessor component = obj.GetComponent<QueryProcessor>();
            component.NetworkPlayerId = testSubjectId++;
            component._ipAddress = "127.0.0.WAN";
            ccm.CurClass = RoleType.None;
            obj.GetComponent<PlayerStats>().SetHPAmount(ccm.Classes.SafeGet(RoleType.None).maxHP);
            obj.GetComponent<NicknameSync>().Network_myNickSync = $"Test subject ({testSubjectId})";
            NetworkServer.Spawn(obj);
            PlayerManager.AddPlayer(obj, CustomNetworkManager.slots);
            Timing.CallDelayed(0.1f, delegate ()
            {
                try
                {
                    Player.UnverifiedPlayers.TryGetValue(ccm._hub, out Player player);
                    Exiled.Events.Handlers.Player.OnJoined(new Exiled.Events.EventArgs.JoinedEventArgs(player));
                    Player.Dictionary.Add(obj, player);
                    player.IsVerified = true;
                    ccm.UserId = "76561198134629649@steam";
                    Exiled.Events.Handlers.Player.OnVerified(new Exiled.Events.EventArgs.VerifiedEventArgs(player));
                    //Npc.Dictionary.Add(obj, null);
                }
                catch (Exception message)
                {
                    Log.Error(message);
                }
            });
            return testSubjectId;
        }
    }

    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = false;
    }

    [HarmonyPatch(typeof(Player), "get_IPAddress")]
    internal static class PlayerIPPatch
    {
        private static bool Prefix(Player __instance, ref string __result)
        {
            __result = __instance.ReferenceHub.queryProcessor._ipAddress;
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.TargetSyncHp))]
    internal static class PlayerStats_TargetSyncHp
    {
        private static bool Prefix(NetworkConnection conn, float hp)
        {
            if (conn == null)
                return false;
            return true;
        }
    }
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.TargetConsolePrint))]
    internal static class CharacterClassManager_TargetConsolePrint
    {
        private static bool Prefix(NetworkConnection connection, string text, string color)
        {
            if (connection == null)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.TargetForcePosition))]
    internal static class PlayerMovementSync_TargetForcePosition
    {
        private static bool Prefix(NetworkConnection conn, Vector3 pos)
        {
            if (conn == null)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.TargetSetRotation))]
    internal static class PlayerMovementSync_TargetSetRotation
    {
        private static bool Prefix(NetworkConnection conn, float rot)
        {
            if (conn == null)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.TargetSetRealId))]
    internal static class CharacterClassManager_TargetSetRealId
    {
        private static bool Prefix(NetworkConnection conn, string userId)
        {
            if (conn == null)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.TargetOpenRemoteAdmin))]
    internal static class ServerRoles_TargetOpenRemoteAdmin
    {
        private static bool Prefix(NetworkConnection connection, bool password)
        {
            if (connection == null)
                return false;
            return true;
        }
    }
}
