using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
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
        public static ItemType GetKeycard()
        {
            /*var tmp = new ItemType[]
            {
                ItemType.KeycardO5, //
                ItemType.KeycardScientist, //
                ItemType.KeycardScientistMajor, //
                ItemType.KeycardSeniorGuard, //
                ItemType.KeycardZoneManager, //
                ItemType.KeycardNTFLieutenant, //
                ItemType.KeycardNTFCommander,
                ItemType.KeycardJanitor, //
                ItemType.KeycardGuard, //
                ItemType.KeycardFacilityManager, //
                ItemType.KeycardContainmentEngineer //
            };*/
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
            if (rand < 80)
                return ItemType.KeycardFacilityManager;
            return ItemType.KeycardO5;
            //return tmp[UnityEngine.Random.Range(0, tmp.Length)];
        }
        internal static void Generate(ItemType card) => Timing.RunCoroutine(_generate(card));
        private static IEnumerator<float> _generate(ItemType card)
        {
            int a = 0;
            foreach (var item in Pickup.Instances)
            {
                if (item.durability == 9991025f)
                {
                    item.Delete();
                    //yield return Timing.WaitForSeconds(0.005f);
                }
            }
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
                        GameObject.DestroyImmediate(gameObject.GetComponent<Collider>());
                        Mirror.NetworkServer.Spawn(gameObject);
                        var keycard = gameObject.GetComponent<Pickup>();
                        keycard.Locked = true;
                        keycard.SetupPickup(card, 9991025f, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
                        a++;
                        if(a % 200 == 0)
                            yield return Timing.WaitForSeconds(0.01f);
                    }
                }
            }
            Log.Debug($"[ColorfullEZ] Spawned {a} keycards");
        }
        private void Server_WaitingForPlayers()
        {
            Generate(GetKeycard());
        }

        public ColorfullEZHandler(IPlugin<IConfig> plugin) : base(plugin)
        {

        }
    }
}
