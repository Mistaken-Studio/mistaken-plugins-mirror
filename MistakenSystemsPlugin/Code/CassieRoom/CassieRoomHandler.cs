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
            //194.37 1002.871 -64.62
            Vector3 pos = new Vector3(194.5f, 998f, -70f);
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = pos;
            gameObject.transform.localScale = new Vector3(2, 2, 2);
            gameObject.transform.rotation = Quaternion.Euler(0, -80, 65);
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            Pickup pickup = gameObject.GetComponent<Pickup>();
            pickup.SetupPickup(ItemType.Flashlight, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            pickup.Locked = true;

            pos = new Vector3(194.5f, 998f, -70f);
            gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = pos;
            gameObject.transform.localScale = new Vector3(2, 2, 2);
            gameObject.transform.rotation = Quaternion.Euler(0, -30, 65);
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            pickup = gameObject.GetComponent<Pickup>();
            pickup.SetupPickup(ItemType.Flashlight, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            pickup.Locked = true;

            pos = new Vector3(189f, 998f, -72.8f);
            gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = pos;
            gameObject.transform.localScale = new Vector3(2, 2, 2);
            gameObject.transform.rotation = Quaternion.Euler(0, 90, 90);
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            pickup = gameObject.GetComponent<Pickup>();
            pickup.SetupPickup(ItemType.Flashlight, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            pickup.Locked = true;
            ItemType keycardType = ItemType.KeycardSeniorGuard;

            //189.1 992.5 -73
            //MainDoor
            var mainDoor = DoorUtils.SpawnDoor(DoorUtils.DoorType.EZ_BREAKABLE, null, new Vector3(189.1f, 992.5f, -73), Vector3.zero, Vector3.one);
            mainDoor.RequiredPermissions.RequiredPermissions = KeycardPermissions.ContainmentLevelThree | KeycardPermissions.ArmoryLevelThree | KeycardPermissions.AlphaWarhead;
            (mainDoor as BreakableDoor)._brokenPrefab = null;
            mainDoor.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            //UpperDoor
            var door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(189.1f, 995.75f, -73), Vector3.zero, Vector3.one);
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(189.1f, 995.75f + 3.25f, -73), Vector3.zero, Vector3.one);
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, new Vector3(189.1f, 995.75f + 3.25f + 3.25f, -73), Vector3.zero, Vector3.one);
            door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (door as BreakableDoor)._brokenPrefab = null;
            //189 999.95 -73 0 0 0 20 1020 2
            pos = new Vector3(189f, 999.95f, -73f);
            gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = pos;
            gameObject.transform.localScale = new Vector3(20, 1020, 2);
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            pickup = gameObject.GetComponent<Pickup>();
            pickup.SetupPickup(keycardType, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            pickup.Locked = true;
            //189 1005 -73 0 0 0 80 500 2
            pos = new Vector3(189f, 1005f, -73f);
            gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = pos;
            gameObject.transform.localScale = new Vector3(80, 500, 2);
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            pickup = gameObject.GetComponent<Pickup>();
            pickup.SetupPickup(keycardType, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            pickup.Locked = true;
            //189 1005 -84.5 90 90 0 100 2500 2
            pos = new Vector3(189f, 1005f, -84.5f);
            gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = pos;
            gameObject.transform.localScale = new Vector3(100, 2500, 2);
            gameObject.transform.rotation = Quaternion.Euler(90, 90, 0);
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            pickup = gameObject.GetComponent<Pickup>();
            pickup.SetupPickup(keycardType, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            pickup.Locked = true;

            foreach (var item in Doors)
            {
                Log.Debug("Spawning Door");
                //Door
                door = DoorUtils.SpawnDoor(DoorUtils.DoorType.HCZ_BREAKABLE, null, item.Pos, item.Rot, item.Size);
                door.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
                (door as BreakableDoor)._brokenPrefab = null;
                //Card
                gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
                gameObject.transform.position = item.Pos - new Vector3(1.65f, 0, 0);
                gameObject.transform.localScale = new Vector3(item.Size.x * 9, item.Size.y * 410, item.Size.z * 2);
                gameObject.transform.rotation = Quaternion.Euler(item.Rot);
                gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                Mirror.NetworkServer.Spawn(gameObject);
                pickup = gameObject.GetComponent<Pickup>();
                pickup.SetupPickup(keycardType, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
                pickup.Locked = true;
                Log.Debug("Spawned Door");
            }
        }

        public static readonly List<(Vector3 Pos, Vector3 Size, Vector3 Rot)> Doors = new List<(Vector3 Pos, Vector3 Size, Vector3 Rot)>()
        {
            //spawn 183.4 993.5 -73 0 0 90 1 1 1
            (new Vector3(183.4f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
            (new Vector3(185f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
            (new Vector3(195f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
            (new Vector3(193.4f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
            (new Vector3(188.1f, 998.5f, -73), new Vector3(6, 1, 1), Vector3.forward * 90),
        };
    }
}
