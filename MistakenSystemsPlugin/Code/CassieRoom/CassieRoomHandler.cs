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
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
        }
       
        private void Server_WaitingForPlayers()
        {
            //CassieRoom-Outside
            SpawnItem(ItemType.Flashlight, new Vector3(194.5f, 998f, -70f), new Vector3(0, -80, 65), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(194.5f, 998f, -70f), new Vector3(0, -30, 65), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(190f, 998f, -72.8f), new Vector3(0,  90, 90), new Vector3(2, 2, 2));
            //Main-Outside
            SpawnItem(ItemType.Flashlight, new Vector3(179f, 998f, -72.8f), new Vector3(0, 90, 45), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(179f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(176f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(182f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(173f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(170f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(184.5f, 992.75f, -68.2f), new Vector3(0, -90, 180), new Vector3(2, 2, 2));

            SpawnItem(ItemType.Flashlight, new Vector3(179f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(176f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(182f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(173f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(170f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));
            SpawnItem(ItemType.Flashlight, new Vector3(184.5f, 992.75f, -51.5f), new Vector3(0, 90, 180), new Vector3(2, 2, 2));

            //150 1010 -45 -40 0 -90 1 1 1
            for (int i = 0; i < 20; i++)
            {
                SpawnItem(ItemType.GunE11SR, new Vector3(150 + i*2, 1010, -45), new Vector3(-40, 0, -90), new Vector3(.01f, .01f, .01f));
            }

            //180 1006 -75 -90 0 90 1 1 1
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    SpawnItem(ItemType.GunE11SR, new Vector3(180 + i * 4, 1005, -75 - j * 4), new Vector3(-90, 0, 90), new Vector3(.01f, .01f, .01f));
                }
            }

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
            SpawnItem(keycardType, new Vector3(190f, 1005f, -73f), new Vector3(0, 0, 0), new Vector3(80, 500, 2));
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
        } 

        public static void SpawnItem(ItemType type, Vector3 pos, Vector3 rot, Vector3 size)
        {
            var gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = pos;
            gameObject.transform.localScale = size;
            gameObject.transform.rotation = Quaternion.Euler(rot);
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            var pickup = gameObject.GetComponent<Pickup>();
            pickup.SetupPickup(type, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 4), gameObject.transform.position, gameObject.transform.rotation);
            pickup.Locked = true;
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
}
