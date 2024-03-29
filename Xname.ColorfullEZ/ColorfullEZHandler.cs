﻿using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Xname.ColorfullEZ
{
    /// <inheritdoc/>
    public class ColorfullEZHandler : Gamer.Diagnostics.Module
    {
        /// <inheritdoc/>
        public override string Name => "ColorfullEZHandler";
        private static new __Log Log;
        /// <inheritdoc/>
        public ColorfullEZHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
            Log = base.Log;
            this.RunCoroutine(Loop(), "Loop");
        }

        private IEnumerator<float> Loop()
        {
            List<NetworkIdentity> tmp = new List<NetworkIdentity>();
            while (true)
            {
                try
                {
                    var start = DateTime.Now;
                    foreach (var player in RealPlayers.List)
                    {
                        if (player.ReferenceHub.networkIdentity.connectionToClient == null)
                            continue;
                        if (SkipFor.Contains(player))
                        {
                            if (!Gamer.Mistaken.Systems.Handler.PlayerPreferencesDict[player.UserId].HasFlag(Gamer.API.PlayerPreferences.DISABLE_COLORFUL_EZ_SPECTATOR_079))
                                SkipFor.Remove(player);
                        }
                        else
                        {
                            if (Gamer.Mistaken.Systems.Handler.PlayerPreferencesDict[player.UserId].HasFlag(Gamer.API.PlayerPreferences.DISABLE_COLORFUL_EZ_SPECTATOR_079))
                                SkipFor.Add(player);
                        }
                        tmp.Clear();
                        if (player.Role == RoleType.Spectator || player.Role == RoleType.Scp079)
                        {
                            if (SkipFor.Contains(player))
                                continue;
                            if (LoadedAll.Contains(player))
                                continue;
                            LoadedAll.Add(player);
                            SyncFor(player);
                        }
                        else
                        {
                            if (LoadedAll.Contains(player))
                            {
                                DesyncFor(player);
                                LoadedAll.Remove(player);
                            }
                            var currentRoom = player.CurrentRoom;
                            if (LastRooms.TryGetValue(player, out var lastRoom) && lastRoom == currentRoom)
                                continue;
                            LastRooms[player] = currentRoom;
                            if (!LoadedFor.TryGetValue(player, out var loadedFor))
                            {
                                loadedFor = new List<NetworkIdentity>();
                                LoadedFor[player] = loadedFor;
                            }

                            if (currentRoom.Zone == ZoneType.Entrance || (currentRoom.Zone == ZoneType.HeavyContainment && HCZRooms.Contains(currentRoom)))
                            {
                                foreach (var item in Keycards)
                                {
                                    if (UnityEngine.Vector3.Distance(item.Key.Position, player.Position) < 50)
                                        tmp.AddRange(item.Value);
                                }
                            }
                            int i = 0;
                            foreach (var netid in loadedFor.ToArray())
                            {
                                if (!tmp.Contains(netid))
                                {
                                    i++;
                                    ObjectDestroyMessage msg = new ObjectDestroyMessage
                                    {
                                        netId = netid.netId
                                    };
                                    NetworkServer.SendToClientOfPlayer<ObjectDestroyMessage>(player.ReferenceHub.networkIdentity, msg);
                                    if (netid.observers.ContainsKey(player.Connection.connectionId))
                                    {
                                        netid.observers.Remove(player.Connection.connectionId);
                                        removeFromVisList?.Invoke(player.Connection, new object[] { netid, true });
                                    }
                                    loadedFor.Remove(netid);
                                }
                            }
                            //Log.Debug($"Unloaded {i} for {player.Nickname}");
                            i = 0;
                            foreach (var netid in tmp)
                            {
                                if (!loadedFor.Contains(netid))
                                {
                                    i++;
                                    MethodInfo sendSpawnMessage = Server.SendSpawnMessage;
                                    sendSpawnMessage.Invoke(null, new object[]
                                    {
                                        netid,
                                        player.Connection
                                    });
                                    loadedFor.Add(netid);
                                }
                            }
                            //Log.Debug($"Loaded {i} for {player.Nickname}");
                        }


                    }
                    Gamer.Diagnostics.MasterHandler.LogTime("ColorfullEZHandler", "Loop", start, DateTime.Now);
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                yield return Timing.WaitForSeconds(1);
            }
        }

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        private void Player_Died(DiedEventArgs ev)
        {
            if (SkipFor.Contains(ev.Target))
                DesyncSyncedFor(ev.Target);
        }

        private void Player_ChangingRole(ChangingRoleEventArgs ev)
        {
            if (SkipFor.Contains(ev.Player) && (ev.NewRole == RoleType.Spectator || ev.NewRole == RoleType.Scp079))
                DesyncSyncedFor(ev.Player);
        }

        internal static readonly Dictionary<Player, List<NetworkIdentity>> LoadedFor = new Dictionary<Player, List<NetworkIdentity>>();
        internal static readonly Dictionary<Player, Room> LastRooms = new Dictionary<Player, Room>();
        internal static readonly HashSet<Room> HCZRooms = new HashSet<Room>();
        internal static readonly HashSet<Player> LoadedAll = new HashSet<Player>();
        internal static readonly HashSet<Player> SkipFor = new HashSet<Player>();
        private void Player_Verified(VerifiedEventArgs ev)
        {
            if (Gamer.Mistaken.Systems.Handler.PlayerPreferencesDict[ev.Player.UserId].HasFlag(Gamer.API.PlayerPreferences.DISABLE_COLORFUL_EZ_SPECTATOR_079))
                SkipFor.Add(ev.Player);
            else
                SkipFor.Remove(ev.Player);
        }

        internal static void SyncFor(Player player)
        {
            MethodInfo sendSpawnMessage = Server.SendSpawnMessage;
            if (sendSpawnMessage != null)
            {
                if (player.ReferenceHub.networkIdentity.connectionToClient == null)
                    return;
                Log.Debug($"Syncing cards for {player.Nickname}");
                foreach (var netid in networkIdentities)
                {
                    sendSpawnMessage.Invoke(null, new object[]
                    {
                        netid,
                        player.Connection
                    });
                }
            }
        }
        private static MethodInfo removeFromVisList = null;
        internal static void DesyncFor(Player player)
        {
            if (removeFromVisList == null)
                removeFromVisList = typeof(NetworkConnection).GetMethod("RemoveFromVisList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (player.ReferenceHub.networkIdentity.connectionToClient == null)
                return;
            Log.Debug($"DeSyncing cards for {player.Nickname}");
            foreach (var netid in networkIdentities)
            {
                ObjectDestroyMessage msg = new ObjectDestroyMessage
                {
                    netId = netid.netId
                };
                NetworkServer.SendToClientOfPlayer<ObjectDestroyMessage>(player.ReferenceHub.networkIdentity, msg);
                if (netid.observers.ContainsKey(player.Connection.connectionId))
                {
                    netid.observers.Remove(player.Connection.connectionId);
                    removeFromVisList?.Invoke(player.Connection, new object[] { netid, true });
                }
            }
            if (LoadedFor.ContainsKey(player))
                LoadedFor[player].Clear();
        }

        internal static void DesyncSyncedFor(Player player)
        {
            if (LoadedFor.TryGetValue(player, out var loadedFor))
            {
                if (removeFromVisList == null)
                    removeFromVisList = typeof(NetworkConnection).GetMethod("RemoveFromVisList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (player.ReferenceHub.networkIdentity.connectionToClient == null)
                    return;
                Log.Debug($"DeSyncing Synced cards for {player.Nickname}");
                foreach (var netid in loadedFor.ToArray())
                {
                    ObjectDestroyMessage msg = new ObjectDestroyMessage
                    {
                        netId = netid.netId
                    };
                    NetworkServer.SendToClientOfPlayer<ObjectDestroyMessage>(player.ReferenceHub.networkIdentity, msg);
                    if (netid.observers.ContainsKey(player.Connection.connectionId))
                    {
                        netid.observers.Remove(player.Connection.connectionId);
                        removeFromVisList?.Invoke(player.Connection, new object[] { netid, true });
                    }
                    loadedFor.Remove(netid);
                }
            }
            else
                Log.Debug($"No cards to DeSync for {player.Nickname}");
        }

        /// <summary>
        /// Returns random keycard
        /// </summary>
        /// <returns>Random keycard</returns>
        public static ItemType GetKeycard()
        {
            int rand = UnityEngine.Random.Range(0, 100);
            if (rand < 2)
                return ItemType.KeycardContainmentEngineer;
            if (rand < 5)
                return ItemType.KeycardNTFLieutenant;
            if (rand < 10)
                return ItemType.KeycardJanitor;
            if (rand < 20)
                return ItemType.KeycardZoneManager;
            if (rand < 30)
                return ItemType.KeycardSeniorGuard;
            if (rand < 40)
                return ItemType.KeycardScientistMajor;
            if (rand < 50)
                return ItemType.KeycardScientist;
            if (rand < 60)
                return ItemType.KeycardGuard;
            if (rand < 70)
                return ItemType.KeycardNTFCommander;
            if (rand < 90)
                return ItemType.KeycardFacilityManager;
            return ItemType.KeycardO5;
        }
        /// <summary>
        /// Removes all cards
        /// </summary>
        public static void Clear()
        {
            foreach (var item in KeycardsGameObjects.ToArray())
                NetworkServer.Destroy(item);
            KeycardsGameObjects.Clear();
            networkIdentities.Clear();
            Keycards.Clear();
            LastRooms.Clear();
            LoadedFor.Clear();
            HCZRooms.Clear();
            LoadedAll.Clear();
            SkipFor.Clear();
        }
        private static readonly Dictionary<Room, List<NetworkIdentity>> Keycards = new Dictionary<Room, List<NetworkIdentity>>();
        private static readonly List<GameObject> KeycardsGameObjects = new List<GameObject>();
        private static readonly List<NetworkIdentity> networkIdentities = new List<NetworkIdentity>();
        /// <summary>
        /// Removes all old generated keycards if present. Generates Colorfull Entrance Zone.
        /// </summary>
        /// <param name="card">Card Type</param>
        public static void Generate(ItemType card) => Gamer.Utilities.BetterCourotines.RunCoroutine(generate(card), "ColorfulEZ.generate");
        private static IEnumerator<float> generate(ItemType card)
        {
            Clear();
            var checkpoint = Map.Rooms.First(r => r.Type == RoomType.HczEzCheckpoint);
            HCZRooms.Add(checkpoint);
            foreach (var room in Map.Rooms)
            {
                if (room.Zone != ZoneType.HeavyContainment)
                    continue;
                if (UnityEngine.Vector3.Distance(checkpoint.Position, room.Position) < 30)
                    HCZRooms.Add(room);
            }

            int a = 0;
            foreach (var roomObject in ColorfullEZManager.keycardRooms)
            {
                foreach (var room in Map.Rooms.Where(x => x.Type == roomObject.Key))
                {
                    Keycards[room] = new List<NetworkIdentity>();
                    Log.Debug($"Spawning {roomObject.Key}, {roomObject.Value.Count} keycards");
                    foreach (var item in roomObject.Value)
                    {
                        var basePos = room.Position;
                        var offset = item.Item1;
                        offset = room.transform.forward * -offset.x + room.transform.right * -offset.z + UnityEngine.Vector3.up * offset.y;
                        basePos += offset;
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
                        gameObject.transform.position = basePos;
                        gameObject.transform.localScale = item.Item2;
                        gameObject.transform.rotation = Quaternion.Euler(room.transform.eulerAngles + item.Item3);
                        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                        gameObject.layer = 0;
                        var keycard = gameObject.GetComponent<Pickup>();
                        keycard.Locked = true;
                        keycard.SetupPickup(card, 9991025f, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
                        foreach (var c in keycard.model.GetComponents<Component>())
                            GameObject.Destroy(c.gameObject);
                        networkIdentities.Add(keycard.netIdentity);
                        Keycards[room].Add(keycard.netIdentity);
                        Pickup.Instances.Remove(keycard);
                        GameObject.Destroy(keycard);
                        foreach (var _item in gameObject.GetComponents<Collider>())
                            GameObject.Destroy(_item);
                        foreach (var _item in gameObject.GetComponents<MeshRenderer>())
                            GameObject.Destroy(_item);
                        Mirror.NetworkServer.Spawn(gameObject);
                        KeycardsGameObjects.Add(gameObject);
                        gameObject.SetActive(false);
                        a++;
                        if (a % 200 == 0)
                            yield return Timing.WaitForSeconds(0.01f);
                    }
                }
            }
            Log.Debug("--------------------------------------");
            PrintComponents(KeycardsGameObjects[0], 0);
            Log.Debug("--------------------------------------");
            Log.Debug($"Spawned {a} keycards");
        }

        private static readonly string reeeeee = "                                                                    ";
        private static void PrintComponents(GameObject go, int iteration)
        {
            foreach (var c in go.GetComponents<Component>())
            {
                Log.Debug(reeeeee.Substring(0, iteration) + c.GetType().Name);
                Log.Debug(reeeeee.Substring(0, iteration) + c.GetType().FullName);
                Log.Debug("------------");
            }
            for (int i = go.transform.childCount; i > 0; i--)
            {
                PrintComponents(go.transform.GetChild(i - 1).gameObject, iteration + 1);
            }
        }
        private void Server_WaitingForPlayers()
        {
            Generate(GetKeycard());
        }
    }
}
