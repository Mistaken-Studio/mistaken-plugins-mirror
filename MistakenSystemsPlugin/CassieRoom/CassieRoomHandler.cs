using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.CustomItems;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Systems.Components;
using Gamer.Utilities;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Gamer.Mistaken.CassieRoom
{
    public class CassieRoomHandler : Diagnostics.Module
    {
        //public override bool Enabled => false;
        public CassieRoomHandler(PluginHandler plugin) : base(plugin)
        {
            this.RunCoroutine(Loop(), "Loop");
        }

        internal static readonly HashSet<Player> LoadedAll = new HashSet<Player>();
        private IEnumerator<float> Loop()
        {
            while (true)
            {
                try
                {
                    var start = DateTime.Now;
                    foreach (var player in RealPlayers.List)
                    {
                        if (player.ReferenceHub.networkIdentity.connectionToClient == null)
                            continue;
                        if (player.Position.y > 900 || player.Role == RoleType.Spectator || player.Role == RoleType.Scp079)
                        {
                            if (LoadedAll.Contains(player))
                                continue;
                            LoadedAll.Add(player);
                            SyncFor(player);
                        }
                        else if (LoadedAll.Contains(player))
                        {
                            DesyncFor(player);
                            LoadedAll.Remove(player);
                        }
                    }

                    Gamer.Diagnostics.MasterHandler.LogTime("CassieRoom", "Loop", start, DateTime.Now);
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                yield return Timing.WaitForSeconds(1);
            }
        }

        internal void SyncFor(Player player)
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
        internal void DesyncFor(Player player)
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
        }

        public override string Name => "CassieRoom";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Warhead.Starting += this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Warhead.Stopping += this.Handle<Exiled.Events.EventArgs.StoppingEventArgs>((ev) => Warhead_Stopping(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Warhead.Starting -= this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Warhead.Stopping -= this.Handle<Exiled.Events.EventArgs.StoppingEventArgs>((ev) => Warhead_Stopping(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            DesyncFor(ev.Player);
        }

        private void Warhead_Starting(Exiled.Events.EventArgs.StartingEventArgs ev)
        {
            WarheadStartButton.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, true);
            WarheadStopButton.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, false);
        }

        private void Warhead_Stopping(Exiled.Events.EventArgs.StoppingEventArgs ev)
        {
            WarheadStartButton.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, false);
            WarheadStopButton.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, true);
        }
        [Flags]
        public enum PluginDoorLockReason : ushort
        {
            COOLDOWN = 512,
            BLOCKED_BY_SOMETHING = 1024,
            REQUIREMENTS_NOT_MET = 2048,
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.IsAllowed && DoorCallbacks.TryGetValue(ev.Door, out var callback))
                ev.IsAllowed = callback(ev);
            else if(ev.Door == mainDoor)
            {
                if (ev.Player.Role != RoleType.NtfCommander)
                    ev.IsAllowed = false;
                else
                {
                    if(!unlocked)
                    {
                        if (ev.Player.GetSessionVar<bool>(Main.SessionVarType.CC_GUARD_COMMANDER) && CustomItemsHandler.GetCustomItem(ev.Player.CurrentItem)?.SessionVarType == Main.SessionVarType.CI_GUARD_COMMANDER_KEYCARD)
                            unlocked = true;
                        else
                            ev.IsAllowed = false;
                    }
                    if (!ev.IsAllowed)
                        return;
                    mainDoor.ServerChangeLock(PluginDoorLockReason.COOLDOWN, true);
                    MEC.Timing.CallDelayed(30f, () =>
                    {
                        mainDoor.ServerChangeLock(PluginDoorLockReason.COOLDOWN, false);
                    });
                }
            }
        }

        private bool unlocked = false;

        public static readonly Dictionary<DoorVariant, Func<Exiled.Events.EventArgs.InteractingDoorEventArgs, bool>> DoorCallbacks = new Dictionary<DoorVariant, Func<Exiled.Events.EventArgs.InteractingDoorEventArgs, bool>>();

        private DoorVariant WarheadStartButton;
        private DoorVariant WarheadStopButton;
        private DoorVariant WarheadLockButton;
        private DoorVariant TeslaToggleButton;
        private DoorVariant CassieRoomOpenButton;
        private DoorVariant mainDoor;
        internal static DoorVariant TeslaIndicator;
        private void Server_WaitingForPlayers()
        {
            networkIdentities.Clear();
            #region Helipad
            SpawnItem(ItemType.KeycardSeniorGuard, new Vector3(18, 40, 0.05f), new Vector3(90, 90, 0), new Vector3(181f, 992.460f, -58.3f + 0.1f));
            SpawnItem(ItemType.KeycardSeniorGuard, new Vector3(30, 40, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.461f, -58.3f - 2f + 0.1f));
            SpawnItem(ItemType.KeycardSeniorGuard, new Vector3(30, 40, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.461f, -58.3f + 2f + 0.1f));
            SpawnItem(ItemType.KeycardGuard, new Vector3(26.25f, 1600, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.45f, -58.3f + 0.1f));
            SpawnItem(ItemType.KeycardGuard, new Vector3(26.25f, 1600, 0.05f), new Vector3(90, 45, 0), new Vector3(181f, 992.451f, -58.3f + 0.1f));
            SpawnItem(ItemType.KeycardGuard, new Vector3(26.25f, 1600, 0.05f), new Vector3(90, 90, 0), new Vector3(181f, 992.452f, -58.3f + 0.1f));
            SpawnItem(ItemType.KeycardGuard, new Vector3(26.25f, 1600, 0.05f), new Vector3(90, 135, 0), new Vector3(181f, 992.453f, -58.3f + 0.1f));

            SpawnItem(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 90, 0), new Vector3(187.446f, 992.453f, -58.3f + 0.1f));
            SpawnItem(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 90, 0), new Vector3(174.56f, 992.453f, -58.3f + 0.1f));
            SpawnItem(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.453f, -58.3f + 0.1f - 6.45f));
            SpawnItem(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.453f, -58.3f + 0.1f + 6.45f));
            SpawnItem(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 45, 0), new Vector3(187.446f - 1.86f, 992.453f, -58.3f + 0.1f + 4.5f));
            SpawnItem(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 45, 0), new Vector3(174.56f + 1.86f, 992.453f, -58.3f + 0.1f - 4.5f));
            SpawnItem(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, -45, 0), new Vector3(187.446f - 1.86f, 992.453f, -58.3f + 0.1f - 4.5f));
            SpawnItem(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, -45, 0), new Vector3(174.56f + 1.86f, 992.453f, -58.3f + 0.1f + 4.5f));
            #endregion
            #region Lights
            {
                SpawnItem(ItemType.Flashlight, new Vector3(179f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));
                SpawnItem(ItemType.Flashlight, new Vector3(176f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));
                SpawnItem(ItemType.Flashlight, new Vector3(182f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));
                SpawnItem(ItemType.Flashlight, new Vector3(184.5f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));

                SpawnItem(ItemType.Flashlight, new Vector3(179f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));
                SpawnItem(ItemType.Flashlight, new Vector3(176f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));
                SpawnItem(ItemType.Flashlight, new Vector3(182f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));
                SpawnItem(ItemType.Flashlight, new Vector3(184.5f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));

                //150 1010 -45 -40 0 -90 1 1 1
                for (int i = 0; i < 22; i++)
                {
                    SpawnItem(ItemType.GunE11SR, new Vector3(150 + i * 2, 1010, -45), new Vector3(-40, 0, -90), new Vector3(.01f, .01f, .01f));
                    if (i > 17)
                        SpawnItem(ItemType.GunE11SR, new Vector3(150 + i * 2, 1010, -45), new Vector3(-35, 0, -90), new Vector3(.01f, .01f, .01f));
                }



                //180 1006 -75 -90 0 90 1 1 1
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        SpawnItem(ItemType.GunE11SR, new Vector3(180 + i * 4, 1005, -75 - j * 4), new Vector3(-90, 0, 90), new Vector3(.01f, .01f, .01f));
                    }
                }
            }
            #endregion
            #region Structure
            {
                ItemType keycardType = ItemType.KeycardContainmentEngineer;
                //MainDoor
                mainDoor = DoorUtils.SpawnDoor(DoorUtils.DoorType.EZ_BREAKABLE, null, new Vector3(190f, 992.5f, -73), Vector3.zero, Vector3.one);
                (mainDoor as BreakableDoor)._brokenPrefab = null;
                mainDoor.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, true);
                Systems.Patches.DoorPatch.IgnoredDoor.Add(mainDoor);
                //UpperDoor
                SpawnDoor(null, new Vector3(190f, 995.75f, -73), Vector3.zero, new Vector3(1, 1, 0.1f));
                SpawnDoor(null, new Vector3(190f, 995.75f + 3.25f, -73), Vector3.zero, new Vector3(1, 1, 0.1f));
                SpawnDoor(null, new Vector3(190f, 995.75f + 3.25f + 3.25f, -73), Vector3.zero, new Vector3(1, 1, 0.1f));
                SpawnItem(keycardType, new Vector3(190f, 999.95f, -73f), new Vector3(0, 0, 0), new Vector3(20, 1020, 2));
                SpawnItem(keycardType, new Vector3(188f, 1005f, -73f), new Vector3(0, 0, 0), new Vector3(70, 500, 2));
                SpawnItem(keycardType, new Vector3(189f, 1005f, -84.5f), new Vector3(90, 90, 0), new Vector3(100, 2500, 2));

                foreach (var (Pos, Size, Rot) in Doors)
                {
                    Log.Debug("Spawning Door");
                    //Door
                    SpawnDoor(null, Pos, Rot, Size);
                    //Card
                    SpawnItem(keycardType, Pos - new Vector3(1.65f, 0, 0), Rot, new Vector3(Size.x * 9, Size.y * 410, Size.z * 2));
                    Log.Debug("Spawned Door");
                }

                var obj = new GameObject();
                var collider = obj.AddComponent<BoxCollider>();
                obj.transform.position = new Vector3(187f, 1005, -83f);
                collider.size = new Vector3(20, 2, 20);
            }
            #endregion
            #region Functionality
            {
                //Test Button
                SpawnButton(new Vector3(181, 994, -75), new Vector3(-1.5f, 2, -2), new Vector3(0, 90, 90), "<size=150%><color=yellow>Test</color> button</size>", (ev) =>
                {
                    Cassie.Message(".g4 .g4 CASSIE ROOM OVERRIDE .g4 .g4 . This is a test message", false, false);
                    ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, true);
                    this.CallDelayed(5, () =>
                    {
                        ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, false);
                    }, "TestButtonCooldown");
                    return false;
                });
                //Warhead Start
                WarheadStartButton = SpawnButton(new Vector3(181, 994, -79), new Vector3(-1.5f, 2, -2), new Vector3(0, 90, 90), "<size=150%><color=yellow>Start</color> a Warhead</size>", (ev) =>
                {
                    Cassie.Message(".g4 .g4 CASSIE ROOM OVERRIDE .g4 .g4 . Warhead engaged", false, false);
                    ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, true);
                    this.CallDelayed(5, () =>
                    {
                        Warhead.Start();
                        this.CallDelayed(2 * 60, () =>
                        {
                            ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, false);
                        }, "WarheadStartCooldown");
                    }, "DelayWarheadStart");
                    return false;
                });
                //Warhead Stop
                WarheadStopButton = SpawnButton(new Vector3(181, 994, -83), new Vector3(-1.5f, 2, -2), new Vector3(0, 90, 90), "<size=150%><color=yellow>Stop</color> a Warhead</size>", (ev) =>
                {
                    if (Warhead.DetonationTimer < 10 + 5)
                        return false;
                    Cassie.Message(".g4 .g4 CASSIE ROOM OVERRIDE .g4 .g4 . Warhead disengaged", false, false);
                    ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, true);
                    this.CallDelayed(5, () =>
                    {
                        Warhead.Stop();
                        this.CallDelayed(2 * 60, () =>
                        {
                            ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, false);
                        }, "WarheadStopCooldown");
                    }, "DelayWarheadStop");
                    return false;
                });
                WarheadStopButton.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, true);
                //Disable warhead
                WarheadLockButton = SpawnButton(new Vector3(181, 994, -87), new Vector3(-1.5f, 2, -2), new Vector3(0, 90, 90), "<size=150%><color=yellow>Disable</color> Warhead</size>", (ev) =>
                {
                    Systems.Misc.BetterWarheadHandler.Warhead.StartLock = true;
                    Cassie.Message(".g4 .g4 CASSIE ROOM OVERRIDE .g4 .g4 . warhead is now in permanent lockdown");
                    WarheadLockButton.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, true);
                    return false;
                });
                TeslaIndicator = SpawnIndicator(new Vector3(181, 994.5f, -91), new Vector3(-1.5f, 2, -2), new Vector3(0, 90, 90));
                TeslaToggleButton = SpawnButton(new Vector3(181, 994, -91), new Vector3(-1.5f, 2, -2), new Vector3(0, 90, 90), "<size=150%><color=yellow>Toggles</color> all Tesla gates</size><br><color=blue>Enabled</color> | <color=green>Disabled</color>", (ev) =>
                {
                    ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, true);
                    if (Systems.Utilities.API.Map.TeslaMode == Systems.Utilities.API.TeslaMode.ENABLED)
                    {
                        Systems.Utilities.API.Map.TeslaMode = Systems.Utilities.API.TeslaMode.DISABLED;
                        TeslaIndicator.NetworkTargetState = true;
                        Cassie.Message(".g4 .g4 CASSIE ROOM OVERRIDE .g4 .g4 . Tesla gates are now deactivated");
                    }
                    else
                    {
                        Systems.Utilities.API.Map.TeslaMode = Systems.Utilities.API.TeslaMode.ENABLED;
                        TeslaIndicator.NetworkTargetState = false;
                        Cassie.Message(".g4 .g4 CASSIE ROOM OVERRIDE .g4 .g4 . Tesla gates are now activated");
                    }
                    MEC.Timing.CallDelayed(2 * 60, () =>
                    {
                        ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, false);
                    });
                    return false;
                });

                CassieRoomOpenButton = SpawnButton(new Vector3(-16.3f, 1020, -48.7f), Vector3.zero, new Vector3(0, 90, 90), "", (ev) =>
                {
                    mainDoor.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, false);
                    CassieRoomOpenButton.ServerChangeLock(PluginDoorLockReason.COOLDOWN, true);
                    CassieRoomOpenButton.NetworkTargetState = true;
                    MEC.Timing.CallDelayed(30f, () =>
                    {
                        CassieRoomOpenButton.NetworkTargetState = false;
                        mainDoor.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, true);
                        CassieRoomOpenButton.ServerChangeLock(PluginDoorLockReason.COOLDOWN, false);
                        if (((PluginDoorLockReason)mainDoor.NetworkActiveLocks & PluginDoorLockReason.BLOCKED_BY_SOMETHING) == 0)
                            mainDoor.NetworkTargetState = false;
                    });
                    return false;
                }, new Vector3(0.5f, 0.5f, 0.5f));
                CassieRoomOpenButton.NetworkTargetState = false;

                //188 992.46 -91 180 0 0 10 0.001 10
                SpawnItem(ItemType.SCP018, new Vector3(188, 992.46f, -91), new Vector3(180, 0, 0), new Vector3(10, 0.001f, 10));
                InRangeBall.Spawn(new Vector3(188, 993, -91), 1, 1, 
                    (player) =>
                    {

                    }, 
                    (player) =>
                    {

                    }
                );
                InRange isSomeoneInside = null;
                isSomeoneInside = InRange.Spawn(new Vector3(188, 993f, -85), new Vector3(23, 10, 23),
                    (player) =>
                    {
                        mainDoor.ServerChangeLock(PluginDoorLockReason.BLOCKED_BY_SOMETHING, true);
                    },
                    (player) =>
                    {
                        this.CallDelayed(5, () =>
                        {
                            if (isSomeoneInside.ColliderInArea.Count == 0)
                            {
                                mainDoor.ServerChangeLock(PluginDoorLockReason.BLOCKED_BY_SOMETHING, false);
                                if (((PluginDoorLockReason)mainDoor.NetworkActiveLocks & PluginDoorLockReason.REQUIREMENTS_NOT_MET) != 0)
                                    mainDoor.NetworkTargetState = false;
                            }
                        }, "Unlock doors");
                    }
                );
            }
            #endregion
        }
        public static DoorVariant SpawnIndicator(Vector3 pos, Vector3 buttonOffset, Vector3 rotation)
        {
            var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, pos + buttonOffset, rotation, Vector3.one);
            (door as BreakableDoor)._brokenPrefab = null;
            door.NetworkTargetState = false;
            DoorCallbacks[door] = (ev) => false;
            Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
            return door;
        }
        public static DoorVariant SpawnButton(Vector3 pos, Vector3 buttonOffset, Vector3 rotation, string name, Func<Exiled.Events.EventArgs.InteractingDoorEventArgs, bool> onCall, Vector3 size = default)
        {
            var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, pos + buttonOffset, rotation, size == default ? Vector3.one : size);
            (door as BreakableDoor)._brokenPrefab = null;
            door.NetworkTargetState = true;
            DoorCallbacks[door] = onCall;
            Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
            Systems.Components.InRange.Spawn(pos, Vector3.one * 2, (p) =>
            {
                p.SetGUI("cassie_room_display", Base.GUI.PseudoGUIHandler.Position.MIDDLE, name);
            }, (p) =>
            {
                p.SetGUI("cassie_room_display", Base.GUI.PseudoGUIHandler.Position.MIDDLE, null);
            });
            return door;
        }
        public static DoorVariant SpawnDoor(string name, Vector3 pos, Vector3 rotation, Vector3 size)
        {
            var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, name, pos, rotation, size);
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
            networkIdentities.Add(door.netIdentity);
            return door;
        }

        private static readonly List<Mirror.NetworkIdentity> networkIdentities = new List<Mirror.NetworkIdentity>();
        public static void SpawnItem(ItemType type, Vector3 pos, Vector3 rot, Vector3 size, bool collide = false)
        {
            var gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = pos;
            gameObject.transform.localScale = size;
            gameObject.transform.rotation = Quaternion.Euler(rot);
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            if (collide)
            {
                gameObject.AddComponent<BoxCollider>();
                gameObject.layer = LayerMask.GetMask("Default");
            }
            var pickup = gameObject.GetComponent<Pickup>();
            pickup.SetupPickup(type, 78253f, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 4), gameObject.transform.position, gameObject.transform.rotation);
            pickup.Locked = true;
            foreach (var c in pickup.model.GetComponents<Component>())
                GameObject.Destroy(c.gameObject);
            networkIdentities.Add(pickup.netIdentity);
            Pickup.Instances.Remove(pickup);
            GameObject.Destroy(pickup);
            foreach (var _item in gameObject.GetComponents<Collider>())
                GameObject.Destroy(_item);
            foreach (var _item in gameObject.GetComponents<MeshRenderer>())
                GameObject.Destroy(_item);
            Mirror.NetworkServer.Spawn(gameObject);
            gameObject.SetActive(false);
        }

        public static readonly List<(Vector3 Pos, Vector3 Size, Vector3 Rot)> Doors = new List<(Vector3 Pos, Vector3 Size, Vector3 Rot)>()
        {
            (new Vector3(183.4f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
            (new Vector3(185f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
            (new Vector3(195f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
            (new Vector3(194.3f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
            (new Vector3(188f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
            (new Vector3(189f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
        };
    }

    public static class Extensions
    {
        public static void ServerChangeLock(this DoorVariant door, CassieRoomHandler.PluginDoorLockReason type, bool active)
        {
            door?.ServerChangeLock((DoorLockReason)type, active);
        }
    }
}
