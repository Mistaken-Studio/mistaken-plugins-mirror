﻿using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Systems.GUI;
using Gamer.Utilities;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.Mistaken.CassieRoom
{
    class Elevator : Module
    {
        //public override bool Enabled => false;
        private new static __Log Log;
        public Elevator(PluginHandler plugin) : base(plugin)
        {
            Log = base.Log;
        }

        public override string Name => "Elevator";

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
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

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.Door == DoorUp || ev.Door == DoorDown)
            {
                ev.IsAllowed = false;
                Timing.RunCoroutine(MoveElevator());
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
            networkIdentities.Clear();

            DoorDown = SpawnElevator(Vector3.zero);
            DoorUp = SpawnElevator(Offset);
            DoorUp.NetworkTargetState = true;
            Spawn1499ContainmentChamber();

            //-26.65 1019.5 -46.5 80 90 90 1 1 1
            GameObject gameObject = UnityEngine.Object.Instantiate(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = new Vector3(-26.65f, 1019.5f, -46.5f);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(80, 90, 90));
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(ItemType.GunE11SR, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 1, 4, 4), gameObject.transform.position, gameObject.transform.rotation);

            gameObject = UnityEngine.Object.Instantiate(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = new Vector3(-26.65f, 1019.5f, -47f);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(80, 90, 90));
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(ItemType.GunE11SR, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 1, 4, 3), gameObject.transform.position, gameObject.transform.rotation);
            
            gameObject = UnityEngine.Object.Instantiate(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = new Vector3(-26.65f, 1019.5f, -47.5f);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(80, 90, 90));
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(ItemType.GunE11SR, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 4, 3, 4), gameObject.transform.position, gameObject.transform.rotation);

            gameObject = UnityEngine.Object.Instantiate(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = new Vector3(-26.65f, 1019.5f, -48f);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(80, 90, 90));
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(ItemType.GunE11SR, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 4, 3, 4), gameObject.transform.position, gameObject.transform.rotation);

        }

        public static DoorVariant SpawnElevator(Vector3 offset)
        {
            ItemType keycardType = ItemType.KeycardNTFLieutenant;

            //Elevator
            //-15 1000 -41 0 90 0 1 1 1
            var elevatorDoor = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(-15f, 1000f, -41.2f) + offset, Vector3.up * 90, Vector3.one);
            //elevatorDoor.RequiredPermissions.RequiredPermissions = KeycardPermissions.ContainmentLevelThree | KeycardPermissions.ArmoryLevelThree | KeycardPermissions.AlphaWarhead;
            (elevatorDoor as BreakableDoor)._brokenPrefab = null;
            Systems.Patches.DoorPatch.IgnoredDoor.Add(elevatorDoor);
            DoorVariant door;
            foreach (var item in ElevatorDoors)
            {
                Log.Debug("Spawning Door");
                //Door
                door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, item.Pos + offset, item.Rot, item.Size);
                door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
                (door as BreakableDoor)._brokenPrefab = null;
                Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
                //Card
                SpawnItem(keycardType, item.Pos - new Vector3(1.65f, 0, 0) + offset, item.Rot, new Vector3(item.Size.x * 9, item.Size.y * 410, item.Size.z * 2));
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
            Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
            //-15 1001.9 -40 0 0 90 1.4 1 1
            door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(-15, 1001.9f, -39.5f) + offset, new Vector3(0, 0, 90), new Vector3(1.4f, 1, 1));
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            Systems.Patches.DoorPatch.IgnoredDoor.Add(door);

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
            Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
            var obj = new GameObject();
            var collider = obj.AddComponent<BoxCollider>();
            obj.transform.position = new Vector3(-16.58f, 1004f, -41f) + offset;
            collider.size = new Vector3(4, 2, 5);
            var inRange = Systems.Components.InRage.Spawn(new Vector3(-16.58f, 1002.7f, -41f) + offset, new Vector3(3f, 4, 4f), (p) =>
            {
                InElevator.Add(p);
            }, (p) =>
            {
                InElevator.Remove(p);
            });
            return elevatorDoor;
        }

        public static DoorVariant Spawn1499ContainmentChamber()
        {
            ItemType keycardType = ItemType.KeycardNTFLieutenant;

            //-23.7 1018.6 -43.5 0 90 0 1 1 1
            var mainDoor = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, "SCP1499Chamber", new Vector3(-23.8f, 1018.6f, -43.5f), Vector3.up * 90, Vector3.one);
            mainDoor.RequiredPermissions.RequiredPermissions = KeycardPermissions.ContainmentLevelThree;
            (mainDoor as BreakableDoor)._brokenPrefab = null;
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

        public static readonly List<Player> InElevator = new List<Player>();
        public static Vector3 BottomMiddle = new Vector3(-16.58f, 1002f, -41f);
        public static bool ElevatorUp = true;
        public static bool Moving = false;
        public static DoorVariant DoorUp;
        public static DoorVariant DoorDown;

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
            if (ElevatorUp)
            {
                foreach (var item in InElevator.ToArray())
                {
                    if (item.IsConnected && item.IsAlive && item.Position.y < 1010)
                        item.Position += Offset;
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
                foreach (var item in InElevator.ToArray())
                {
                    if (item.IsConnected && item.IsAlive && item.Position.y > 1010)
                        item.Position -= Offset;
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
            //foreach (var c in pickup.model.GetComponents<Component>())
            //    GameObject.Destroy(c.gameObject);
            //networkIdentities.Add(pickup.netIdentity);
            Pickup.Instances.Remove(pickup);
            //GameObject.Destroy(pickup);
            //foreach (var _item in gameObject.GetComponents<Collider>())
            //    GameObject.Destroy(_item);
            //foreach (var _item in gameObject.GetComponents<MeshRenderer>())
            //    GameObject.Destroy(_item);
            Mirror.NetworkServer.Spawn(gameObject);
            //gameObject.SetActive(false);
        }

        public static DoorVariant SpawnDoor(Vector3 pos, Vector3 rot, Vector3 size)
        {
            var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, pos, rot, size);
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            Systems.Patches.DoorPatch.IgnoredDoor.Add(door);
            return door;
        }
    }
}