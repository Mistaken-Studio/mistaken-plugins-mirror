using Exiled.API.Features;
using Mirror;
using NorthwoodLib.Pools;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Gamer.Utilities
{
    /// <summary>
    /// Map Extensions but not as extensionb because <see cref="Map"/> is static
    /// </summary>
    public static class MapPlus
    {
        /// <summary>
        /// If SCP 106 someone lured
        /// </summary>
        public static bool Lured
        {
            get
            {
                return GameObject.FindObjectOfType<LureSubjectContainer>().NetworkallowContain;
            }
            set
            {
                GameObject.FindObjectOfType<LureSubjectContainer>().SetState(value);
            }
        }
        /// <summary>
        /// If SCP 106 Containment was used
        /// </summary>
        public static bool FemurBreaked
        {
            get
            {
                return OneOhSixContainer.used;
            }
            set
            {
                OneOhSixContainer.used = value;
            }
        }
        /// <summary>
        /// Send Broadcast
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <param name="duration">Duration</param>
        /// <param name="message">Message</param>
        /// <param name="flags">Flags</param>
        public static void Broadcast(string tag, ushort duration, string message, Broadcast.BroadcastFlags flags = global::Broadcast.BroadcastFlags.Normal)
        {
            if (flags == global::Broadcast.BroadcastFlags.AdminChat)
            {
                string fullMessage = $"<color=orange>[<color=green>{tag}</color>]</color> {message}";
                foreach (var item in RealPlayers.List?.Where(p => p.Connection != null && PermissionsHandler.IsPermitted(p.ReferenceHub.serverRoles.Permissions, PlayerPermissions.AdminChat)) ?? new List<Player>())
                    item.ReferenceHub.queryProcessor.TargetReply(item.Connection, "@" + fullMessage, true, false, string.Empty);
            }
            else
                Map.Broadcast(duration, $"<color=orange>[<color=green>{tag}</color>]</color> {message}", flags);
        }
        /// <summary>
        /// Spawns items with args
        /// </summary>
        /// <param name="item">Item info</param>
        /// <param name="position">Spawn position</param>
        /// <param name="rotation">Spawn rotation</param>
        /// <param name="size">Pickup size</param>
        /// <returns>Spawned object</returns>
        public static Pickup Spawn(Inventory.SyncItemInfo item, Vector3 position, Quaternion rotation, Vector3 size)
        {
            var inv = Server.Host.Inventory;
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(inv.pickupPrefab);
            var originalScale = gameObject.transform.localScale;
            gameObject.transform.localScale = new Vector3(size.x * originalScale.x, size.y * originalScale.y, size.z * originalScale.z);
            NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(item.id, item.durability, inv.gameObject, new Pickup.WeaponModifiers(true, item.modSight, item.modBarrel, item.modOther), position, rotation);
            return gameObject.GetComponent<Pickup>();
        }
        /// <summary>
        /// Spawns dummy
        /// </summary>
        /// <param name="role">Role</param>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="size">Size</param>
        /// <param name="Name">Name</param>
        /// <returns>Dummy</returns>
        public static GameObject SpawnDummy(RoleType role, Vector3 position, Quaternion rotation, Vector3 size, string Name)
        {
            GameObject obj = GameObject.Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
            if (ccm == null)
                Log.Error("[SPAWN DUMMY] CCM is null");
            ccm.CurClass = role;
            //ccm.RefreshPlyModel(role);
            obj.GetComponent<NicknameSync>().Network_myNickSync = Name;
            obj.GetComponent<QueryProcessor>().PlayerId = 99999;
            obj.GetComponent<QueryProcessor>().NetworkPlayerId = 99999;
            obj.transform.localScale = size;
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            NetworkServer.Spawn(obj);

            return obj;
        }
        /// <summary>
        /// Time to decontamination end
        /// </summary>
        public static float DecontaminationEndTime
        {
            get
            {
                var lastPhase = LightContainmentZoneDecontamination.DecontaminationController.Singleton.DecontaminationPhases.First(i => i.Function == LightContainmentZoneDecontamination.DecontaminationController.DecontaminationPhase.PhaseFunction.Final);
                return (float)lastPhase.TimeTrigger;
            }
        }
        /// <summary>
        /// Is LCZ Decontaminated
        /// </summary>
        /// <param name="minTimeLeft">Offset</param>
        /// <returns>If LCZ is Decontaminated</returns>
        public static bool IsLCZDecontaminated(float minTimeLeft = 0)
        {
            var lczTime = DecontaminationEndTime - (float)LightContainmentZoneDecontamination.DecontaminationController.GetServerTime;
            return lczTime < minTimeLeft;
        }
        /// <summary>
        /// If LCZ was decontaminated with out param
        /// </summary>
        /// <param name="lczTime">Time to decontamination</param>
        /// <param name="minTimeLeft">Offset</param>
        /// <returns>If LCZ is decontaminated</returns>
        public static bool IsLCZDecontaminated(out float lczTime, float minTimeLeft = 0)
        {
            lczTime = DecontaminationEndTime - (float)LightContainmentZoneDecontamination.DecontaminationController.GetServerTime;
            return lczTime < minTimeLeft;
        }

        private static (ReadOnlyCollection<Room> Rooms, int round) _rooms = (new ReadOnlyCollection<Room>(new List<Room>()), -1);
        /// <summary>
        /// Rooms List
        /// </summary>
        public static IEnumerable<Room> Rooms
        {
            get
            {
                if ((Map.Rooms?.Where(r => r != null).Count() ?? 0) == 0)
                {
                    if (_rooms.round == RoundPlus.RoundId)
                    {
                        Log.Debug("Returning Rooms from Cache");
                        return _rooms.Rooms;
                    }
                    Log.Warn("Running Rooms Cache Code");
                    var tmpRooms = GameObject.FindObjectsOfType<Room>();
                    if (tmpRooms.Length > 0)
                    {
                        Log.Info("Running Rooms Cache Code");
                        _rooms = (tmpRooms.ToList().AsReadOnly(), RoundPlus.RoundId);
                        return _rooms.Rooms;
                    }
                    Log.Warn("Running Rooms Cache Code");
                    List<GameObject> list = ListPool<GameObject>.Shared.Rent();
                    list.AddRange(GameObject.FindGameObjectsWithTag("Room"));
                    if (list.Count == 0)
                    {
                        ListPool<GameObject>.Shared.Return(list);
                        throw new InvalidOperationException("Plugin is trying to access Rooms before they are created.");
                    }
                    GameObject gameObject = GameObject.Find("HeavyRooms/PocketWorld");
                    if (gameObject != null)
                        list.Add(gameObject);
                    GameObject gameObject2 = GameObject.Find("Outside");
                    if (gameObject2 != null)
                        list.Add(gameObject2);
                    List<Room> tor = new List<Room>();
                    foreach (GameObject roomGameObject in list)
                    {
                        if (roomGameObject.TryGetComponent<Room>(out Room room))
                            tor.Add(room);
                        else
                            tor.Add(roomGameObject.AddComponent<Room>());
                    }
                    ListPool<GameObject>.Shared.Return(list);
                    _rooms = (tor.AsReadOnly(), RoundPlus.RoundId);
                    return _rooms.Rooms;
                }
                return Map.Rooms.Where(r => r != null);
            }
        }
    }
}
