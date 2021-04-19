using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Mistaken.Systems.Staff;
using Gamer.Utilities;
using Interactables.Interobjects.DoorUtils;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))] 
    class DevTestCommand : IBetterCommand
    {       
        public override string Description => "DEV STUFF";
        public override string Command => "test";
        internal static Dictionary<Player, Pickup> keycard = new Dictionary<Player, Pickup>();
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (!player.IsActiveDev())
                return new string[] { "This command is used for testing, allowed only for Mistaken Devs" };
            switch (args[0])
            {
                case "sound":
                    GameObject.FindObjectOfType<AmbientSoundPlayer>().RpcPlaySound(int.Parse(args[1]));
                    break;
                case "tfc":
                    player.ChangeAppearance(RealPlayers.Get(args[1]), (RoleType)sbyte.Parse(args[2]));
                    break;
                case "fc":
                    player.ChangeAppearance((RoleType)sbyte.Parse(args[1]));
                    break;
                case "nick":
                    player.TargetSetNickname(RealPlayers.Get(args[1]), args[2]);
                    break;
                case "badge":
                    player.TargetSetBadge(RealPlayers.Get(args[1]), args[2], args[3]);
                    break;
                case "give":
                    Inventory.SyncItemInfo info;
                    switch (args[1].ToLower())
                    {
                        case "taser":
                            info = new Inventory.SyncItemInfo
                            {
                                id = ItemType.GunUSP,
                                durability = 1.501f + (int.Parse(args[2]) / 1000000)
                            };
                            break;
                        case "impact":
                            info = new Inventory.SyncItemInfo
                            {
                                id = ItemType.GrenadeFrag,
                                durability = 1.001f
                            };
                            break;
                        case "armor":
                            info = new Inventory.SyncItemInfo
                            {
                                id = ItemType.Coin,
                                durability = 1.001f + (int.Parse(args[2]) / 1000000)
                            };
                            break;
                        case "snav-3000":
                            info = new Inventory.SyncItemInfo
                            {
                                id = ItemType.WeaponManagerTablet,
                                durability = 1.301f
                            };
                            break;
                        case "snav-ultimate":
                            info = new Inventory.SyncItemInfo
                            {
                                id = ItemType.GrenadeFrag,
                                durability = 1.401f
                            };
                            break;
                        case "scp-1499":
                            info = new Inventory.SyncItemInfo
                            {
                                id = ItemType.GrenadeFlash,
                                durability = 1.149f
                            };
                            break;
                        default:
                            return new string[] { "Avaiable items:", "Taser", "Impact", "Armor", "SNav-3000", "SNav-Ultimate", "SCP-1499" };
                    }
                    if(player.Inventory.items.Count > 7)
                        info.Spawn(player.Position);
                    else
                        player.AddItem(info);
                    break;
                case "spawn":
                    {
                        try
                        {
                            if (keycard.ContainsKey(player))
                                keycard[player]?.Delete();
                        }
                        catch { }
                        var basePos = player.CurrentRoom.Position;
                        var offset = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        offset = player.CurrentRoom.transform.forward * -offset.x + player.CurrentRoom.transform.right * -offset.z + Vector3.up * offset.y;
                        basePos += offset;
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
                        gameObject.transform.position = basePos;
                        gameObject.transform.localScale = new Vector3(float.Parse(args[7]), float.Parse(args[8]), float.Parse(args[9]));
                        gameObject.transform.rotation = Quaternion.Euler(player.CurrentRoom.transform.eulerAngles + new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6])));
                        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                        Mirror.NetworkServer.Spawn(gameObject);
                        keycard[player] = gameObject.GetComponent<Pickup>();
                        keycard[player].SetupPickup(ItemType.KeycardFacilityManager, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
                        return new string[] { player.CurrentRoom.Type + "", basePos.x + "", basePos.y + "", basePos.z + "", player.CurrentRoom.Type.ToString() + "" };
                    }
                case "heh":
                    {
                        var p = Player.Get(args[1]);
                        //Explore(p.GameObject.transform);
                        var size = new Vector3(float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]));
                        var tmp = Find(p.GameObject.transform, "Body");
                        foreach (var item in tmp.Take(tmp.Count - 1))
                        {
                            item.localScale = size;
                        }
                        Log.Debug(":|");
                        foreach (Player _p in Player.List)
                        {
                            MethodInfo sendSpawnMessage = Server.SendSpawnMessage;
                            if (sendSpawnMessage != null)
                            {
                                sendSpawnMessage.Invoke(null, new object[]
                                {
                                p.ReferenceHub.characterClassManager.netIdentity,
                                _p.Connection
                                });
                            }
                            else
                                Log.Debug("FOCK OFF");
                        }
                        break;
                    }
                case "builder":
                    {
                        var pos = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        var rotation = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]));
                        var size = new Vector3(float.Parse(args[7]), float.Parse(args[8]), float.Parse(args[9]));
                        DoorVariant doorVariant = UnityEngine.Object.Instantiate(DoorUtils.GetPrefab(DoorUtils.DoorType.HCZ_BREAKABLE), pos, Quaternion.Euler(rotation));
                        doorVariant.gameObject.transform.localScale = size;
                        GameObject.Destroy(doorVariant);
                        NetworkServer.Spawn(doorVariant.gameObject);
                        break;
                    }
                case "aaa":
                    string _arg = args[1] == "null" ? null : args[1];
                    player.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = (byte)(_arg == null ? 0 : 1);
                    player.ReferenceHub.characterClassManager.NetworkCurUnitName = _arg;
                    break;
                case "wall":
                    {
                        var pos = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        var rot = new Vector3(float.Parse(args[4]) - 180, float.Parse(args[5]) - 180, float.Parse(args[6]));
                        var width = float.Parse(args[7]);
                        var height = float.Parse(args[8]);
                        SpawnWorkStation(pos - Vector3.right * 0.05f, rot, new Vector3(width, height, 0.1f));
                        SpawnWorkStation(pos + Vector3.right * 0.05f, new Vector3(rot.x, rot.y + 180, rot.z), new Vector3(width, height, 0.1f));

                        SpawnWorkStation(pos - Vector3.right * 0.05f, new Vector3(rot.x + 180, rot.y, rot.z), new Vector3(width, height, 0.1f));
                        SpawnWorkStation(pos + Vector3.right * 0.05f, new Vector3(rot.x + 180, rot.y + 180, rot.z), new Vector3(width, height, 0.1f));
                        break;
                    }
            }
            success = true;
            return new string[] { "HMM" };
        }

        private WorkStation prefab;
        private void SpawnWorkStation(Vector3 pos, Vector3 rot, Vector3 size)
        {
            if (prefab == null)
            {
                foreach (var item in NetworkManager.singleton.spawnPrefabs)
                {
                    var ws = item.GetComponent<WorkStation>();
                    if (ws)
                    {
                        Log.Debug(item.name);
                        prefab = ws;
                    }
                }
            }
            var spawned = GameObject.Instantiate(prefab.gameObject, pos, Quaternion.Euler(rot));
            spawned.transform.localScale = size;
            Log.Debug("Spawning");
            NetworkServer.Spawn(spawned);
        }

        public void Explore(Transform obj)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
                Explore(obj.transform.GetChild(i));
            Log.Debug(obj);
        }
        public List<Transform> Find(Transform obj, string name)
        {
            var tor = new List<Transform>();
            for (int i = 0; i < obj.transform.childCount; i++)
                tor.AddRange(Find(obj.transform.GetChild(i), name));
            if (obj.name == name || true)
                tor.Add(obj);
            return tor;
        }
    }
}
