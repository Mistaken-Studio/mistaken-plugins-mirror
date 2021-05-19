using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.Components;
using Gamer.Mistaken.Base.GUI;
using Gamer.Utilities;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Mistaken.CassieRoom
{
    internal class Elevator : Gamer.Diagnostics.Module
    {
        //public override bool Enabled => false;
        private static new __Log Log;
        public Elevator(PluginHandler plugin) : base(plugin)
        {
            Log = base.Log;
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
                    if (netid == null)
                        continue;
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
        public override string Name => "Elevator";

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            CamperPoints[ev.Target] = 0;
            if (CamperEffects.ContainsKey(ev.Target))
                CamperEffects[ev.Target].Clear();
        }
        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            CamperPoints[ev.Player] = 0;
            if (CamperEffects.ContainsKey(ev.Player))
                CamperEffects[ev.Player].Clear();
        }
        private void Server_RoundStarted()
        {
            this.RunCoroutine(DoRoundLoop(), "RoundLoop");
        }
        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            DesyncFor(ev.Player);
        }
        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.Door == DoorUp || ev.Door == DoorDown)
            {
                ev.IsAllowed = false;
                this.RunCoroutine(MoveElevator(), "MoveElevator");
            }
        }

        public static readonly List<(Vector3 Pos, Vector3 Size, Vector3 Rot)> ElevatorDoors = new List<(Vector3 Pos, Vector3 Size, Vector3 Rot)>()
        {
            // -15 1001 -42.5 0 0 90 2 1 1
            (new Vector3(-15f, 1001.34f, -43.1f), new Vector3(2, 1, 1), Vector3.forward * 90),
        };
        public static readonly Vector3 Offset = new Vector3(-8.77f, 18.56f, 0.9f);
        private void Server_WaitingForPlayers()
        {
            Moving = false;
            networkIdentities.Clear();
            ElevatorUp = true;

            (DoorDown, DownTrigger) = SpawnElevator(Vector3.zero);
            (DoorUp, UpTrigger) = SpawnElevator(Offset);
            DoorUp.NetworkTargetState = true;
            Spawn1499ContainmentChamber();

            //-26.65 1019.5 -46.5 80 90 90 1 1 1
            GameObject gameObject = UnityEngine.Object.Instantiate(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = new Vector3(-26.65f, 1019.5f, -46.5f);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(80, 90, 90));
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(ItemType.GunE11SR, 40, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 1, 4, 4), gameObject.transform.position, gameObject.transform.rotation);

            gameObject = UnityEngine.Object.Instantiate(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = new Vector3(-26.65f, 1019.5f, -47f);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(80, 90, 90));
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(ItemType.GunE11SR, 40, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 1, 4, 3), gameObject.transform.position, gameObject.transform.rotation);

            gameObject = UnityEngine.Object.Instantiate(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = new Vector3(-26.65f, 1019.5f, -47.5f);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(80, 90, 90));
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(ItemType.GunE11SR, 40, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 4, 3, 4), gameObject.transform.position, gameObject.transform.rotation);

            gameObject = UnityEngine.Object.Instantiate(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = new Vector3(-26.65f, 1019.5f, -48f);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(80, 90, 90));
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(ItemType.GunE11SR, 40, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 4, 3, 4), gameObject.transform.position, gameObject.transform.rotation);


            //Spawn Killer
            inRange = InRange.Spawn(new Vector3(-20, 1019, -43), new Vector3(20, 5, 20), null, null);

            //Lights
            //-19.5 1021.5 -52 -20 180 0 .01 .01 .01
            //SpawnItem(ItemType.GunE11SR, new Vector3(-19.5f, 1021.5f, -52), new Vector3(-20, 180, 0), Vector3.one * 0.01f);
            //-13.2 1021.5 -45.5 -20 90 0 .01 .01 .01
            //SpawnItem(ItemType.GunE11SR, new Vector3(-13.2f, 1021.5f, -45.5f), new Vector3(-20, 90, 0), Vector3.one * 0.01f);
            //Workstation
            /*var hid = Map.Doors.First(i => i.Type() == Exiled.API.Enums.DoorType.HID);
            foreach (var item in GameObject.FindObjectsOfType<WorkStation>())
            {
                if(Vector3.Distance(hid.transform.position, item.transform.position) < 10)
                {
                    item.transform.position = new Vector3(-18, 1019.7f, -39);
                    item.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
            }*/
        }
        private InRange inRange;
        private readonly Dictionary<Player, int> CamperPoints = new Dictionary<Player, int>();
        private readonly Dictionary<Player, HashSet<Type>> CamperEffects = new Dictionary<Player, HashSet<Type>>();
        private IEnumerator<float> DoRoundLoop()
        {
            yield return Timing.WaitForSeconds(1);
            while(Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(5);
                foreach (var player in RealPlayers.List)
                {
                    if (player.IsDead)
                        continue;
                    if (!CamperPoints.TryGetValue(player, out int value))
                    {
                        CamperPoints[player] = 0;
                        value = 0;
                    }
                    if (inRange.ColliderInArea.Contains(player.GameObject))
                    {
                        if (!CamperEffects.TryGetValue(player, out var effects))
                        {
                            effects = new HashSet<Type>();
                            CamperEffects[player] = effects;
                        }
                        CamperPoints[player] += 2 * 5;
                        value += 2 * 5;
                        //player.SetGUI("Test", PseudoGUIHandler.Position.TOP, "Value: " + value);
                        if(value >= 120) // 1 Min
                        {
                            if (!player.GetEffectActive<CustomPlayerEffects.Deafened>())
                            {
                                effects.Add(typeof(CustomPlayerEffects.Deafened));
                                player.EnableEffect<CustomPlayerEffects.Deafened>();
                            }
                            if (!player.GetEffectActive<CustomPlayerEffects.Disabled>())
                            {
                                effects.Add(typeof(CustomPlayerEffects.Disabled));
                                player.EnableEffect<CustomPlayerEffects.Disabled>();

                                player.SetGUI("Tower_Bad", PseudoGUIHandler.Position.MIDDLE, "Nie czuję się za dobrze.", 5);
                            }

                            if(value >= 180) // 1.5 Min
                            {
                                if (!player.GetEffectActive<CustomPlayerEffects.Concussed>())
                                {
                                    effects.Add(typeof(CustomPlayerEffects.Concussed));
                                    player.EnableEffect<CustomPlayerEffects.Concussed>();

                                    player.SetGUI("Tower_Bad", PseudoGUIHandler.Position.MIDDLE, "Zaczyna mnie boleć głowa.", 5);
                                }

                                if(value >= 240) // 2 Min
                                {
                                    if (!player.GetEffectActive<CustomPlayerEffects.Blinded>())
                                    {
                                        effects.Add(typeof(CustomPlayerEffects.Blinded));
                                        player.EnableEffect<CustomPlayerEffects.Blinded>();
                                    }
                                    if (!player.GetEffectActive<CustomPlayerEffects.Exhausted>())
                                    {
                                        effects.Add(typeof(CustomPlayerEffects.Exhausted));
                                        player.EnableEffect<CustomPlayerEffects.Exhausted>();

                                        player.SetGUI("Tower_Bad", PseudoGUIHandler.Position.MIDDLE, "Jestem taki zmęczony.", 5);
                                    }

                                    if(value >= 360) // 3 Min
                                    {
                                        if (!player.GetEffectActive<CustomPlayerEffects.Hemorrhage>())
                                        {
                                            effects.Add(typeof(CustomPlayerEffects.Hemorrhage));
                                            player.EnableEffect<CustomPlayerEffects.Hemorrhage>();
                                        }
                                        if (!player.GetEffectActive<CustomPlayerEffects.Asphyxiated>())
                                        {
                                            effects.Add(typeof(CustomPlayerEffects.Asphyxiated));
                                            player.EnableEffect<CustomPlayerEffects.Asphyxiated>();
                                        }
                                        if (!player.GetEffectActive<CustomPlayerEffects.Amnesia>())
                                        {
                                            effects.Add(typeof(CustomPlayerEffects.Amnesia));
                                            player.EnableEffect<CustomPlayerEffects.Amnesia>();
                                        }
                                        if (!player.GetEffectActive<CustomPlayerEffects.Bleeding>())
                                        {
                                            effects.Add(typeof(CustomPlayerEffects.Bleeding));
                                            player.EnableEffect<CustomPlayerEffects.Bleeding>();

                                            player.SetGUI("Tower_Bad", PseudoGUIHandler.Position.MIDDLE, "Tracę czucie w nogach.", 5);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (value > 0)
                        {
                            CamperPoints[player] -= 5;
                            value -= 1 * 5;
                            //player.SetGUI("Test", PseudoGUIHandler.Position.TOP, "Value: " + value);
                            if (!CamperEffects.TryGetValue(player, out var effects))
                                continue;
                            if (value < 360) // 3 Min
                            {
                                if (effects.Contains(typeof(CustomPlayerEffects.Hemorrhage)))
                                {
                                    effects.Remove(typeof(CustomPlayerEffects.Hemorrhage));
                                    player.DisableEffect<CustomPlayerEffects.Hemorrhage>();
                                }
                                if (effects.Contains(typeof(CustomPlayerEffects.Asphyxiated)))
                                {
                                    effects.Remove(typeof(CustomPlayerEffects.Asphyxiated));
                                    player.DisableEffect<CustomPlayerEffects.Asphyxiated>();
                                }
                                if (effects.Contains(typeof(CustomPlayerEffects.Amnesia)))
                                {
                                    effects.Remove(typeof(CustomPlayerEffects.Amnesia));
                                    player.DisableEffect<CustomPlayerEffects.Amnesia>();
                                }
                                if (effects.Contains(typeof(CustomPlayerEffects.Bleeding)))
                                {
                                    effects.Remove(typeof(CustomPlayerEffects.Bleeding));
                                    player.DisableEffect<CustomPlayerEffects.Bleeding>();
                                }

                                if (value < 240) // 2 Min
                                {
                                    if (effects.Contains(typeof(CustomPlayerEffects.Blinded)))
                                    {
                                        effects.Remove(typeof(CustomPlayerEffects.Blinded));
                                        player.DisableEffect<CustomPlayerEffects.Blinded>();
                                    }
                                    if (effects.Contains(typeof(CustomPlayerEffects.Exhausted)))
                                    {
                                        effects.Remove(typeof(CustomPlayerEffects.Exhausted));
                                        player.DisableEffect<CustomPlayerEffects.Exhausted>();
                                    }

                                    if (value < 180) // 1.5 Min
                                    {
                                        if (effects.Contains(typeof(CustomPlayerEffects.Concussed)))
                                        {
                                            effects.Remove(typeof(CustomPlayerEffects.Concussed));
                                            player.DisableEffect<CustomPlayerEffects.Concussed>();
                                        }

                                        if (value < 120) // 1 Min
                                        {
                                            if (effects.Contains(typeof(CustomPlayerEffects.Deafened)))
                                            {
                                                effects.Remove(typeof(CustomPlayerEffects.Deafened));
                                                player.DisableEffect<CustomPlayerEffects.Deafened>();
                                            }
                                            if (effects.Contains(typeof(CustomPlayerEffects.Disabled)))
                                            {
                                                effects.Remove(typeof(CustomPlayerEffects.Disabled));
                                                player.DisableEffect<CustomPlayerEffects.Disabled>();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static (DoorVariant door, InRange trigger) SpawnElevator(Vector3 offset)
        {
            ItemType keycardType = ItemType.KeycardNTFLieutenant;

            //Elevator
            //-15 1000 -41 0 90 0 1 1 1
            var elevatorDoor = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(-15f, 1000f, -41.2f) + offset, Vector3.up * 90, Vector3.one);
            //elevatorDoor.RequiredPermissions.RequiredPermissions = KeycardPermissions.ContainmentLevelThree | KeycardPermissions.ArmoryLevelThree | KeycardPermissions.AlphaWarhead;
            (elevatorDoor as BreakableDoor)._brokenPrefab = null;
            Gamer.Mistaken.Base.Patches.DoorPatch.IgnoredDoor.Add(elevatorDoor);
            //networkIdentities.Add(elevatorDoor.netIdentity);

            DoorVariant door;
            foreach (var (Pos, Size, Rot) in ElevatorDoors)
            {
                Log.Debug("Spawning Door");
                //Door
                door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, Pos + offset, Rot, Size);
                door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
                (door as BreakableDoor)._brokenPrefab = null;
                Gamer.Mistaken.Base.Patches.DoorPatch.IgnoredDoor.Add(door);
                networkIdentities.Add(door.netIdentity);
                //Card
                SpawnItem(keycardType, Pos - new Vector3(1.65f, 0, 0) + offset, Rot, new Vector3(Size.x * 9, Size.y * 410, Size.z * 2));
                Log.Debug("Spawned Door");
            }

            //spawn3 -15 1001.5 -42.1 0 90 0 1 400 1
            SpawnItem(keycardType, new Vector3(-15f, 1001.5f, -42.7f) + offset, new Vector3(0, 90, 0), new Vector3(4.75f, 450, 2), true);
            //-15 1001.5 -39.9 0 90 0 3 450 2
            SpawnItem(keycardType, new Vector3(-15f, 1001.5f, -39.9f) + offset, new Vector3(0, 90, 0), new Vector3(3, 450, 2), true);

            //-15 1001.34 -43 0 90 90 2 0.25 1
            door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(-15, 1001.34f, -43) + offset, new Vector3(0, 90, 90), new Vector3(2, 0.25f, 1));
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            Gamer.Mistaken.Base.Patches.DoorPatch.IgnoredDoor.Add(door);
            networkIdentities.Add(door.netIdentity);
            //-15 1001.9 -40 0 0 90 1.4 1 1
            door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(-15, 1001.9f, -39.5f) + offset, new Vector3(0, 0, 90), new Vector3(1.4f, 1, 1));
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            Gamer.Mistaken.Base.Patches.DoorPatch.IgnoredDoor.Add(door);
            networkIdentities.Add(door.netIdentity);

            //-16.58 1003.7 -41 90 180 0 15 550 6
            SpawnItem(keycardType, new Vector3(-16.58f, 1003.7f, -41f) + offset, new Vector3(90, 180, 0), new Vector3(15, 550, 6), true); //Up

            //-16.58 1001.87 -43 0 0 0 15 550 6
            SpawnItem(keycardType, new Vector3(-16.58f, 1001.87f, -43f) + offset, new Vector3(0, 0, 0), new Vector3(15, 550, 6), true); //Up-Left

            //-16.58 1001.87 -39.5 0 0 0 15 550 6
            SpawnItem(keycardType, new Vector3(-16.58f, 1001.87f, -39.5f) + offset, new Vector3(0, 0, 0), new Vector3(15, 550, 6), true); //Right

            //-18.08 1001.87 -41 0 90 0 15 550 6
            SpawnItem(keycardType, new Vector3(-18.08f, 1001.87f, -41f) + offset, new Vector3(0, 90, 0), new Vector3(15, 550, 6), true); //Back

            //-17.78 1001.47 -42.5 0 90 90 1.5 1.2 1 //Back
            door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(-17.78f, 1001.47f, -42.5f) + offset, new Vector3(0, 90, 90), new Vector3(1.5f, 1.1f, 1));
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            Gamer.Mistaken.Base.Patches.DoorPatch.IgnoredDoor.Add(door);
            networkIdentities.Add(door.netIdentity);
            var obj = new GameObject();
            var collider = obj.AddComponent<BoxCollider>();
            obj.transform.position = new Vector3(-16.58f, 1004f, -41f) + offset;
            collider.size = new Vector3(4, 2, 5);
            var elevatorTrigger = Gamer.Mistaken.Base.Components.InRange.Spawn(
                new Vector3(-16.58f, 1002.7f, -41f) + offset, 
                new Vector3(3f, 4, 4f), 
                (p) => 
                { 
                    Log.Debug($"{p.Nickname} entered");
                }, 
                (p) => 
                {
                    Log.Debug($"{p.Nickname} exited");
                }
            );
            //elevatorTrigger.DEBUG = true;
            return (elevatorDoor, elevatorTrigger);
        }
        public static DoorVariant Spawn1499ContainmentChamber()
        {
            ItemType keycardType = ItemType.KeycardNTFLieutenant;

            //-23.7 1018.6 -43.5 0 90 0 1 1 1
            var mainDoor = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, "SCP1499Chamber", new Vector3(-23.8f, 1018.6f, -43.5f), Vector3.up * 90, Vector3.one);
            mainDoor.RequiredPermissions.RequiredPermissions = KeycardPermissions.ContainmentLevelThree;
            (mainDoor as BreakableDoor)._brokenPrefab = null;
            //networkIdentities.Add(mainDoor.netIdentity);
            //Systems.Patches.DoorPatch.IgnoredDoor.Add(mainDoor);

            //-23.7 1022.35 -43.5 0 90 0 10 110 1
            SpawnItem(keycardType, new Vector3(-23.8f, 1022.33f, -43.5f), new Vector3(0, 90, 0), new Vector3(10, 110, 2), true);
            //-23.8 1020.35 -41.92 0 90 0 5.5 610 2
            SpawnItem(keycardType, new Vector3(-23.8f, 1020.35f, -41.92f), new Vector3(0, 90, 0), new Vector3(5.5f, 610, 2), true);
            //-23.8 1020.35 -45.1 0 90 0 5.5 610 2
            SpawnItem(keycardType, new Vector3(-23.8f, 1020.35f, -45.1f), new Vector3(0, 90, 0), new Vector3(5.5f, 610, 2), true);
            //-23.8 1020.35 -45.7 0 90 90 2.2 0.37 1
            SpawnDoor(new Vector3(-23.8f, 1020.35f, -45.7f), new Vector3(0, 90, 90), new Vector3(2.2f, 0.37f, 1));
            //-23.8 1020.35 -45.55 0 0 90 2.2 1 1
            SpawnDoor(new Vector3(-23.8f, 1020.35f, -45.55f), new Vector3(0, 0, 90), new Vector3(2.2f, 1, 1));
            //-25.5 1020.35 -45.55 0 0 0 15 650 2.5
            SpawnItem(keycardType, new Vector3(-25.5f, 1020.35f, -45.55f), new Vector3(0, 0, 0), new Vector3(15f, 650, 2.5f), true);

            //door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(-17.78f, 1001.47f, -42.5f), new Vector3(0, 90, 90), new Vector3(1.5f, 1.1f, 1));
            //door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            //(door as BreakableDoor)._brokenPrefab = null;
            //Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
            return mainDoor;
        }

        public static Vector3 BottomMiddle = new Vector3(-16.58f, 1002f, -41f);
        public static bool ElevatorUp = true;
        public static bool Moving = false;
        public static DoorVariant DoorUp;
        public static DoorVariant DoorDown;
        public static InRange UpTrigger;
        public static InRange DownTrigger;

        public static IEnumerator<float> MoveElevator()
        {
            if (Moving)
                yield break;
            ElevatorUp = !ElevatorUp;
            Moving = true;
            DoorUp.NetworkTargetState = false;
            DoorDown.NetworkTargetState = false;
            DoorDown.ServerChangeLock(DoorLockReason.AdminCommand, true);
            DoorUp.ServerChangeLock(DoorLockReason.AdminCommand, true);
            yield return Timing.WaitForSeconds(3);
            Log.Debug($"Colliders in up trigger: {UpTrigger.ColliderInArea.Count} and in down trigger: {DownTrigger.ColliderInArea.Count}");
            if (ElevatorUp)
            {
                foreach (var item in DownTrigger.ColliderInArea.ToArray())
                {
                    try
                    {
                        var ply = Player.Get(item);
                        if (ply.IsConnected && ply.IsAlive && ply.Position.y < 1010)
                        {
                            ply.Position += Offset;
                            Gamer.RoundLoggerSystem.RoundLogger.Log("ELEVATOR", "TELEPORT", $"Teleported {ply.Nickname} Up ({ply.Position})");
                        }
                        else
                            Gamer.RoundLoggerSystem.RoundLogger.Log("ELEVATOR", "DENY", $"Denied teleporting {ply.Nickname} Up ({ply.Position})");
                    }
                    catch(System.Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }
                foreach (var item in Pickup.Instances)
                {
                    if (item.durability == 78253f)
                        continue;
                    if (Vector3.Distance(item.position, BottomMiddle) < 3)
                        item.transform.position += Offset;
                }
                foreach (var item in GameObject.FindObjectsOfType<Grenades.Grenade>())
                {
                    if (item.thrower == null)
                        continue;
                    if (Vector3.Distance(item.transform.position, BottomMiddle) < 3)
                        item.transform.position += Offset;
                }
                yield return Timing.WaitForSeconds(2);
                DoorDown.NetworkTargetState = false;
                DoorUp.NetworkTargetState = true;
            }
            else
            {
                foreach (var item in UpTrigger.ColliderInArea.ToArray())
                {
                    try
                    {
                        var ply = Player.Get(item);
                        if (ply.IsConnected && ply.IsAlive && ply.Position.y > 1010)
                        {
                            ply.Position -= Offset;
                            Gamer.RoundLoggerSystem.RoundLogger.Log("ELEVATOR", "TELEPORT", $"Teleported {ply.Nickname} Down ({ply.Position})");
                        }
                        else
                            Gamer.RoundLoggerSystem.RoundLogger.Log("ELEVATOR", "DENY", $"Denied teleporting {ply.Nickname} Down ({ply.Position})");
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }
                foreach (var item in Pickup.Instances)
                {
                    if (item.durability == 78253f)
                        continue;
                    if (Vector3.Distance(item.position, BottomMiddle + Offset) < 3)
                        item.transform.position -= Offset;
                }
                foreach (var item in GameObject.FindObjectsOfType<Grenades.Grenade>())
                {
                    if (item.thrower == null)
                        continue;
                    if (Vector3.Distance(item.transform.position, BottomMiddle + Offset) < 3)
                        item.transform.position -= Offset;
                }
                yield return Timing.WaitForSeconds(2);
                DoorDown.NetworkTargetState = true;
                DoorUp.NetworkTargetState = false;
            }
            yield return Timing.WaitForSeconds(2);
            DoorDown.ServerChangeLock(DoorLockReason.AdminCommand, false);
            DoorUp.ServerChangeLock(DoorLockReason.AdminCommand, false);
            Moving = false;
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

        public static DoorVariant SpawnDoor(Vector3 pos, Vector3 rot, Vector3 size)
        {
            var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, pos, rot, size);
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            Gamer.Mistaken.Base.Patches.DoorPatch.IgnoredDoor.Add(door);
            networkIdentities.Add(door.netIdentity);
            return door;
        }
    }
}
