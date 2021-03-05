using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Exiled.API.Features;
using Mirror;
using NorthwoodLib.Pools;
using RemoteAdmin;
using UnityEngine;

namespace Gamer.Utilities
{
    public static class MapPlus
    {
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
        public static void Broadcast(string tag, ushort duration, string message, Broadcast.BroadcastFlags flags = global::Broadcast.BroadcastFlags.Normal)
        {
            if (flags == global::Broadcast.BroadcastFlags.AdminChat)
            {
                string fullMessage = $"<color=orange>[<color=green>{tag}</color>]</color> {message}";
                foreach (var item in RealPlayers.List?.Where(p => PermissionsHandler.IsPermitted(p.ReferenceHub.serverRoles.Permissions, PlayerPermissions.AdminChat)) ?? new List<Player>())
                    item.ReferenceHub.queryProcessor.TargetReply(item.Connection, "@" + fullMessage, true, false, string.Empty);
            }
            else
                Map.Broadcast(duration, $"<color=orange>[<color=green>{tag}</color>]</color> {message}", flags);
        }

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

        public static GameObject SpawnDummy(RoleType role, Vector3 position, Quaternion rotation, Vector3 size, string Name)
        {
            GameObject obj = GameObject.Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
            if (ccm == null)
                Logger.Error("SPAWN DUMMY", "CCM is null");
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

        public static float DecontaminationEndTime
        {
            get
            {
                var lastPhase = LightContainmentZoneDecontamination.DecontaminationController.Singleton.DecontaminationPhases.First(i => i.Function == LightContainmentZoneDecontamination.DecontaminationController.DecontaminationPhase.PhaseFunction.Final);
                return (float)lastPhase.TimeTrigger;
            }
        }

        public static bool IsLCZDecontaminated(float minTimeLeft = 0)
        {
            var lczTime = DecontaminationEndTime - (float)LightContainmentZoneDecontamination.DecontaminationController.GetServerTime;
            return lczTime < minTimeLeft;
        }

        public static bool IsLCZDecontaminated(out float lczTime, float minTimeLeft = 0)
        {
            lczTime = DecontaminationEndTime - (float)LightContainmentZoneDecontamination.DecontaminationController.GetServerTime;
            return lczTime < minTimeLeft;
        }

        private static (ReadOnlyCollection<Room> Rooms, int round) _rooms = (new ReadOnlyCollection<Room>(new List<Room>()), -1);
        public static IEnumerable<Room> Rooms
        {
            get
            {
                if((Map.Rooms?.Where(r => r != null).Count() ?? 0) == 0)
                {
                    if(_rooms.round == RoundPlus.RoundId)
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
