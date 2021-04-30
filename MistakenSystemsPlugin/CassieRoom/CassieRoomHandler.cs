using System.Collections.Generic;
using UnityEngine;
using MEC;
using System;
using System.Linq;
using Gamer.Mistaken.BetterRP.Ambients;
using Exiled.API.Features;
using Exiled.API.Extensions;
using Mirror;
using Gamer.Utilities;
using Gamer.Diagnostics;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Gamer.Mistaken.Systems.GUI;
using LightContainmentZoneDecontamination;

namespace Gamer.Mistaken.CassieRoom
{
    public class CassieRoomHandler : Module
    {
        public CassieRoomHandler(PluginHandler plugin) : base(plugin)
        {
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
            System.Reflection.MethodInfo sendSpawnMessage = Server.SendSpawnMessage;
            if (sendSpawnMessage != null)
            {
                Log.Debug("Syncing cards");
                foreach (var netid in networkIdentities)
                {
                    sendSpawnMessage.Invoke(null, new object[]
                    {
                        netid,
                        ev.Player.Connection
                    });
                }
            }
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
        public enum PluginDoorLockReason: ushort
        {
            COOLDOWN = 512,
            NO_CLOSE_ENOUGHT = 1024,
            REQUIREMENTS_NOT_MET = 2048,
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.IsAllowed && DoorCallbacks.TryGetValue(ev.Door, out var callback))
                ev.IsAllowed = callback(ev);
        }

        public static readonly Dictionary<DoorVariant, Func<Exiled.Events.EventArgs.InteractingDoorEventArgs, bool>> DoorCallbacks = new Dictionary<DoorVariant, Func<Exiled.Events.EventArgs.InteractingDoorEventArgs, bool>>();

        private DoorVariant WarheadStartButton;
        private DoorVariant WarheadStopButton;

        private void Server_WaitingForPlayers()
        {
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
                var mainDoor = DoorUtils.SpawnDoor(DoorUtils.DoorType.EZ_BREAKABLE, null, new Vector3(190f, 992.5f, -73), Vector3.zero, Vector3.one);
                mainDoor.RequiredPermissions.RequiredPermissions = KeycardPermissions.ContainmentLevelThree | KeycardPermissions.ArmoryLevelThree | KeycardPermissions.AlphaWarhead;
                (mainDoor as BreakableDoor)._brokenPrefab = null;
                mainDoor.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
                Systems.Patches.DoorPatch.IgnoredDoor.Add(mainDoor);
                //UpperDoor
                var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(190f, 995.75f, -73), Vector3.zero, Vector3.one);
                door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
                (door as BreakableDoor)._brokenPrefab = null;
                Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
                door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(190f, 995.75f + 3.25f, -73), Vector3.zero, Vector3.one);
                door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
                (door as BreakableDoor)._brokenPrefab = null;
                Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
                door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(190f, 995.75f + 3.25f + 3.25f, -73), Vector3.zero, Vector3.one);
                door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
                (door as BreakableDoor)._brokenPrefab = null;
                Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
                SpawnItem(keycardType, new Vector3(190f, 999.95f, -73f), new Vector3(0, 0, 0), new Vector3(20, 1020, 2));
                SpawnItem(keycardType, new Vector3(188f, 1005f, -73f), new Vector3(0, 0, 0), new Vector3(70, 500, 2));
                SpawnItem(keycardType, new Vector3(189f, 1005f, -84.5f), new Vector3(90, 90, 0), new Vector3(100, 2500, 2));

                foreach (var item in Doors)
                {
                    Log.Debug("Spawning Door");
                    //Door
                    door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, item.Pos, item.Rot, item.Size);
                    door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
                    (door as BreakableDoor)._brokenPrefab = null;
                    Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
                    //Card
                    SpawnItem(keycardType, item.Pos - new Vector3(1.65f, 0, 0), item.Rot, new Vector3(item.Size.x * 9, item.Size.y * 410, item.Size.z * 2));
                    Log.Debug("Spawned Door");
                }

                //View blocker
                SpawnItem(ItemType.KeycardO5, new Vector3(190f, 994.7f, -73f), new Vector3(90, 0, 0), new Vector3(8, 4, 5));

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
                    MEC.Timing.CallDelayed(5, () =>
                    {
                        ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, false);
                    });
                    return false;
                });
                //Warhead Start
                WarheadStartButton = SpawnButton(new Vector3(181, 994, -79), new Vector3(-1.5f, 2, -2), new Vector3(0, 90, 90), "<size=150%><color=yellow>Start</color> a Warhead</size>", (ev) =>
                {
                    Cassie.Message(".g4 .g4 CASSIE ROOM OVERRIDE .g4 .g4 . Warhead engaged", false, false);
                    ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, true);
                    MEC.Timing.CallDelayed(6, () =>
                    {
                        Warhead.Start();
                        MEC.Timing.CallDelayed(2 * 60, () =>
                        {
                            ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, false);
                        });
                    });
                    return false;
                });
                //Warhead Stop
                WarheadStopButton = SpawnButton(new Vector3(181, 994, -83), new Vector3(-1.5f, 2, -2), new Vector3(0, 90, 90), "<size=150%><color=yellow>Stop</color> a Warhead</size>", (ev) =>
                {
                    if (Warhead.DetonationTimer < 10 + 6)
                        return false;
                    Cassie.Message(".g4 .g4 CASSIE ROOM OVERRIDE .g4 .g4 . Warhead disengaged", false, false);
                    ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, true);
                    MEC.Timing.CallDelayed(6, () =>
                    {
                        Warhead.Stop();
                        MEC.Timing.CallDelayed(2 * 60, () =>
                        {
                            ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, false);
                        });
                    });
                    return false;
                });
                WarheadStopButton.ServerChangeLock(PluginDoorLockReason.REQUIREMENTS_NOT_MET, true);
                //Delay Decontamination
                WarheadStartButton = SpawnButton(new Vector3(181, 994, -87), new Vector3(-1.5f, 2, -2), new Vector3(0, 90, 90), "<size=150%><color=yellow>Delay</color> by <color=yellow>5 minutes</color> LCZ Decontamination</size>", (ev) =>
                {
                    Cassie.Message(".g4 .g4 CASSIE ROOM OVERRIDE .g4 .g4 . LIGHT CONTAINMENT ZONE DECONTAMINATION TIME INCREASED BY 5 MINUTES");
                    ev.Door.ServerChangeLock(PluginDoorLockReason.COOLDOWN, true);
                    DecontaminationController.Singleton.NetworkRoundStartTime += 300;
                    return false;
                });
            }
            #endregion
        }

        public static DoorVariant SpawnButton(Vector3 pos, Vector3 buttonOffset, Vector3 rotation, string name, Func<Exiled.Events.EventArgs.InteractingDoorEventArgs, bool> onCall)
        {
            var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, pos + buttonOffset, rotation, Vector3.one);
            door.NetworkTargetState = true;
            DoorCallbacks[door] = onCall;
            Systems.Components.InRage.Spawn(pos, Vector3.one * 2, (p) =>
            {
                Systems.GUI.PseudoGUIHandler.Set(p, "cassie_room_display", PseudoGUIHandler.Position.MIDDLE, name);
            }, (p) => 
            {
                Systems.GUI.PseudoGUIHandler.Set(p, "cassie_room_display", PseudoGUIHandler.Position.MIDDLE, null);
            });
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
            door.ServerChangeLock((DoorLockReason)type, active);
        }
    }
}
