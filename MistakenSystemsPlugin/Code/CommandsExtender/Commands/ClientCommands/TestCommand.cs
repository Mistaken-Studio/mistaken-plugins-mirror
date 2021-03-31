using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
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
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (!player.IsDev())
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
                case "spawn":
                    var basePos = player.CurrentRoom.Position;
                    var offset = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                    offset = player.CurrentRoom.transform.forward * -offset.x + player.CurrentRoom.transform.right * -offset.z + Vector3.up * offset.y;
                    basePos += offset;
                    ItemType.SCP018.Spawn(0, basePos);
                    return new string[] { player.CurrentRoom.Type + "", basePos.x + "", basePos.y + "", basePos.z + "" };
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
                    player.referenceHub.characterClassManager.NetworkCurSpawnableTeamType = (byte)(_arg == null ? 0 : 1);
                    player.referenceHub.characterClassManager.NetworkCurUnitName = _arg;
                    break;
            }
            success = true;
            return new string[] { "HMM" };
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
