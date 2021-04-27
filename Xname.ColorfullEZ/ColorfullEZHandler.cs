using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using Gamer.Diagnostics;
using MEC;
using Mirror;
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
        }

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }

        private void Player_Verified(VerifiedEventArgs ev)
        {
            if ((Gamer.Mistaken.Systems.Handler.PlayerPreferencesDict[ev.Player.UserId] & Gamer.API.PlayerPreferences.DISABLE_COLORFUL_EZ) != Gamer.API.PlayerPreferences.NONE)
                return;
            MethodInfo sendSpawnMessage = Server.SendSpawnMessage;
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
        }
        private static readonly List<GameObject> KeycardsGameObjects = new List<GameObject>();
        private static readonly List<NetworkIdentity> networkIdentities = new List<NetworkIdentity>();
        /// <summary>
        /// Removes all old generated keycards if present. Generates Colorfull Entrance Zone.
        /// </summary>
        /// <param name="card">Card Type</param>
        public static void Generate(ItemType card) => Timing.RunCoroutine(generate(card));
        private static IEnumerator<float> generate(ItemType card)
        {
            int a = 0;
            Clear();
            foreach (var roomObject in ColorfullEZManager.keycardRooms)
            {
                foreach (var room in Map.Rooms.Where(x => x.Type == roomObject.Key))
                {
                    Log.Debug($"Spawning {roomObject.Key}, {roomObject.Value.Count} keycards");
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
                        gameObject.layer = 0;
                        var keycard = gameObject.GetComponent<Pickup>();
                        keycard.Locked = true;
                        keycard.SetupPickup(card, 9991025f, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
                        foreach (var c in keycard.model.GetComponents<Component>())
                            GameObject.Destroy(c.gameObject);
                        networkIdentities.Add(keycard.netIdentity);
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
