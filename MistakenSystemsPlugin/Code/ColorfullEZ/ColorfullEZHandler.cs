using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using Gamer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.Mistaken.ColorfullEZ
{
    public class ColorfullEZHandler : Module
    {
        public override string Name => "ColorfullEZHandler";

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
            var tmp = new ItemType[]
            {
                ItemType.KeycardO5,
                ItemType.KeycardScientist,
                ItemType.KeycardScientistMajor,
                ItemType.KeycardSeniorGuard,
                ItemType.KeycardZoneManager,
                ItemType.KeycardNTFLieutenant,
                ItemType.KeycardNTFCommander,
                ItemType.KeycardJanitor,
                ItemType.KeycardGuard,
                ItemType.KeycardFacilityManager,
                ItemType.KeycardContainmentEngineer
            };
            var card = tmp[UnityEngine.Random.Range(0, tmp.Length)];
            int a = 0;
            foreach (var roomObject in ColorfullEZManager.keycardRooms)
            {
                foreach (var room in MapPlus.Rooms.Where(x => x.Type == roomObject.Key))
                {
                    Log.Debug($"[ColorfullEZ] Spawning {roomObject.Key}, {roomObject.Value.Count} keycards");
                    foreach (var item in roomObject.Value)
                    {

                        var basePos = room.Position;
                        var offset = item.Item1;
                        offset = room.transform.forward * -offset.x + room.transform.right * -offset.z + Vector3.up * offset.y;
                        basePos += offset;
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
                        gameObject.transform.position = basePos;
                        gameObject.transform.localScale = item.Item2;
                        gameObject.transform.rotation = Quaternion.Euler(room.transform.eulerAngles + item.Item3);
                        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                        Mirror.NetworkServer.Spawn(gameObject);
                        var keycard = gameObject.GetComponent<Pickup>();
                        keycard.Locked = true;
                        keycard.SetupPickup(card, 999f, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
                        a++;
                    }
                }
            }
            Log.Debug($"[ColorfullEZ] Spawned {a} keycards");
        }

        public ColorfullEZHandler(IPlugin<IConfig> plugin) : base(plugin)
        {

        }
    }
}
