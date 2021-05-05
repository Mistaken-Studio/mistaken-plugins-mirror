using CustomPlayerEffects;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.Diagnostics;
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
            
            base.OnDisabled();
        }

        public override void OnEnabled()
        {
            IdleMode.PauseIdleMode = true;
            

            Exiled.Events.Events.DisabledPatchesHashSet.Add(typeof(PlayerPositionManager).GetMethod("TransmitData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            Exiled.Events.Events.DisabledPatchesHashSet.Add(typeof(PlayableScps.Scp096).GetMethod(nameof(PlayableScps.Scp096.AddTarget)));
            Exiled.Events.Events.Instance.ReloadDisabledPatches();
            var harmony = new HarmonyLib.Harmony("gamer.citester");
            harmony.PatchAll();

            new CIModule(this);
            Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }
    }

    public class CIModule : Module
    {
        public CIModule(IPlugin<IConfig> plugin) : base(plugin)
        {
        }

        public override string Name => "CITester";

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        private void Server_RestartingRound()
        {
            if(success)
            {
                Log.Info("!! Test was successful !!");
                MEC.Timing.CallDelayed(1, () => Environment.Exit(0));
            }
            else
            {
                Log.Error("!! Test FAILED !!");
                MEC.Timing.CallDelayed(1, () => Environment.Exit(1));
            }
        }
        private bool success = false;
        private void Server_RoundStarted()
        {
            Timing.CallDelayed(1, () =>
            {
                try
                {
                    var player1 = Player.Get(2);
                    if (player1 == null)
                        throw new Exception("Player 1 not found");
                    var player2 = Player.Get(3);
                    if (player2 == null)
                        throw new Exception("Player 1 not found");
                    player1.Role = RoleType.Scp173;
                    player2.Role = RoleType.ClassD;
                    player2.ReferenceHub.weaponManager.curWeapon = 0;
                    player1.ReferenceHub.weaponManager.curWeapon = 0;
                    player1.Hurt(10000000000, player2, DamageTypes.E11StandardRifle);
                    if (player1.IsAlive)
                        throw new Exception("Player 1 did not die");
                    player1.Role = RoleType.NtfCommander;
                    if (player1.Role != RoleType.NtfCommander)
                        throw new Exception("Player 1 did not forceclass");
                    player1.Hurt(120, player2, DamageTypes.E11StandardRifle);
                    if (!player1.IsAlive)
                        throw new Exception("Player 1 died when he shouldn't");
                    player1.EnableEffect<Bleeding>();
                    if (!player1.GetEffectActive<Bleeding>())
                        throw new Exception("Player 1 don't have bleeding effect when he should");
                    player2.DisplayNickname = "Test";
                    if (player2.GetDisplayName() != "Test")
                        throw new Exception("Player 2 nickname didn't change");
                    player2.Kill(DamageTypes.Wall);
                    if (player2.IsAlive)
                        throw new Exception("Player 2 did not die|2");
                    Round.IsLocked = false;
                    MEC.Timing.CallDelayed(1, () =>
                    {
                        try
                        {
                            if (Round.IsStarted)
                                throw new Exception("Round didn't end");
                            //ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Shutdown;
                            success = true;
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error(ex.Message);
                            Log.Error(ex.StackTrace);
                            MasterHandler.LogError(ex, this, "RoundStart");
                        }
                    });
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                    MasterHandler.LogError(ex, this, "RoundStart");
                    Round.IsLocked = false;
                }
            });
        }

        private void Server_WaitingForPlayers()
        {
            SpawnTestObject("76561198134629649@steam");
            SpawnTestObject("barwa@northwood");
            Timing.CallDelayed(1f, () =>
            {
                try
                {
                    if (Player.List.Count() != 2)
                        throw new Exception("Unexpected player count, expected 2 but got " + Player.List.Count());
                    if (RealPlayers.List.Count() != 2)
                        throw new Exception("Unexpected real players count, expected 2 but got " + RealPlayers.List.Count());
                    GameCore.RoundStart.singleton.NetworkTimer = 2;
                }
                catch (System.Exception ex)
                {
                    MasterHandler.LogError(ex, this, "WaitingForPlayers");
                    Round.IsLocked = false;
                }
            });
        }

        private static int testSubjectId = 2;
        public int SpawnTestObject(string userId)
        {
            testSubjectId++;
            Exiled.Events.Handlers.Player.OnPreAuthenticating(new Exiled.Events.EventArgs.PreAuthenticatingEventArgs(userId, null, 0, 0, "PL", true));
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
                    ccm.UserId = userId;
                    Exiled.Events.Handlers.Player.OnVerified(new Exiled.Events.EventArgs.VerifiedEventArgs(player));
                    //Npc.Dictionary.Add(obj, null);
                    player.ReferenceHub.inventory._weaponManager.modPreferences = new int[10, 3];
                    if (player.ReferenceHub == null)
                        throw new Exception("RH is null");
                    if (player.ReferenceHub.inventory == null)
                        throw new Exception("INV is null");
                    if (player.ReferenceHub.inventory._ccm == null)
                        throw new Exception("INV's CCM is null");
                    if (player.ReferenceHub.inventory._ccm._hub == null)
                        throw new Exception("INV's CCM's hub is null");
                    if (player.ReferenceHub.inventory._weaponManager == null)
                        throw new Exception("INV's WM is null");
                    if (player.ReferenceHub.inventory._weaponManager.weapons == null)
                        throw new Exception("INV's WM's weapons is null");
                    if (player.ReferenceHub.inventory._weaponManager.modPreferences == null)
                        throw new Exception("INV's WM's modPreferences is null");
                    if (player.ReferenceHub.inventory.items == null)
                        throw new Exception("INV's items is null");
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    MasterHandler.LogError(ex, this, "SpawningDummy");
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

    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.TargetAchieve))]
    internal static class PlayerStats_TargetAchieve
    {
        private static bool Prefix(NetworkConnection conn, string key)
        {
            if (conn == null)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.TargetSetHiddenRole))]
    internal static class ServerRoles_TargetSetHiddenRole
    {
        private static bool Prefix(NetworkConnection connection, string role)
        {
            if (connection == null)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.TargetBloodEffect))]
    internal static class PlayerStats_TargetBloodEffect
    {
        private static bool Prefix(NetworkConnection conn, Vector3 pos, float overall)
        {
            if (conn == null)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(MirrorExtensions), nameof(MirrorExtensions.SendFakeSyncVar))]
    internal static class MirrorExtensions_SendFakeSyncVar
    {
        private static bool Prefix(Player target, NetworkIdentity behaviorOwner, Type targetType, string propertyName, object value)
        {
            if (target.Connection == null)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.AddTarget))]
    internal static class PlayableScps_Scp096_AddTarget
    {
        private static bool Prefix(GameObject target)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.OnDamage))]
    internal static class PlayableScps_Scp096_OnDamage
    {
        private static bool Prefix(PlayerStats.HitInfo info)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.TargetSyncGameplayData))]
    internal static class QueryProcessor_TargetSyncGameplayData
    {
        private static bool Prefix(NetworkConnection conn, bool gd)
        {
            if (conn == null)
                return false; //A
            return true;
        }
    }

    [HarmonyPatch(typeof(VersionCheck), nameof(VersionCheck.TargetCheckVersion))]
    internal static class VersionCheck_TargetCheckVersion
    {
        private static bool Prefix(NetworkConnection conn, byte major, byte minor, byte revison)
        {
            if (conn == null)
                return false;
            return true;
        }
    }
}
