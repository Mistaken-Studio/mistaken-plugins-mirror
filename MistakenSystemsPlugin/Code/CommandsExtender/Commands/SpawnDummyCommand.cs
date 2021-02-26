

using System.Linq;
using UnityEngine;

using Mirror;
using System.Collections.Generic;
using Gamer.Utilities;
using CommandSystem;
using Exiled.API.Features;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class SpawnDummyCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "spawndummy";

        public override string Description =>
        "Spawns Dummy";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "spawndummy";

        public override string[] Aliases => new string[] { "sdummy" };

        public string GetUsage()
        {
            return "SpawnDummy RAW/TARGET/DELETE (argumets) (autoDestroyInterval (seconds))";
        }

        private uint NextId = 0;
        private readonly Dictionary<uint, GameObject> Dummys = new Dictionary<uint, GameObject>();

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };

            GameObject dummy = null;

            var dummyId = NextId;
            NextId++;

            switch (args[0].ToUpper())
            {
                case "RAW":
                    {
                        float pos_x, pos_y, pos_z;
                        float rot_x, rot_y, rot_z;
                        float size_x, size_y, size_z;
                        RoleType role;
                        string Name;
                        bool success = false;

                        if (float.TryParse(args[1], out pos_x))
                        {
                            if (float.TryParse(args[2], out pos_y))
                            {
                                if (float.TryParse(args[3], out pos_z))
                                {
                                    if (float.TryParse(args[4], out rot_x))
                                    {
                                        if (float.TryParse(args[5], out rot_y))
                                        {
                                            if (float.TryParse(args[6], out rot_z))
                                            {
                                                if (float.TryParse(args[7], out size_x))
                                                {
                                                    if (float.TryParse(args[8], out size_y))
                                                    {
                                                        if (float.TryParse(args[9], out size_z))
                                                        {
                                                            if (int.TryParse(args[10], out int roleId))
                                                            {
                                                                role = (RoleType)roleId;
                                                                Name = args[11];
                                                                dummy = MapPlus.SpawnDummy(role, new Vector3(pos_x, pos_y, pos_z), Quaternion.Euler(rot_x, rot_y, rot_z), new Vector3(size_x, size_y, size_z), Name);
                                                                success = true;

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!success) return new string[] { "SpawnDummy RAW [pos_x] [pos_y] [pos_z] [rot_x] [rot_y] [rot_z] [size_x] [size_y] [size_z] [role] [name]" };
                        break;
                    }

                case "TARGET":
                    {
                        this.ForeachPlayer(args[1], out bool success, (Player player) =>
                        {
                            string name = player.Nickname;
                            if (args.Length > 2 && args[2] != "default")
                                name = args[2];
                            dummy = MapPlus.SpawnDummy(player.Role, player.Position, Quaternion.Euler(player.Rotation), Vector3.one, name);
                            return new string[] { "Done" };
                        });
                        if (!success)
                        {
                            var player = sender.GetPlayer();
                            string name = player.Nickname;
                            if (args.Length > 1 && args[1] != "default")
                                name = args[1];
                            dummy = MapPlus.SpawnDummy(player.Role, player.Position, Quaternion.Euler(player.Rotation), Vector3.one, name);
                        }
                        break;
                    }
                case "DELETE":
                    {
                        if (args.Length > 1 && uint.TryParse(args[1], out uint id) && Dummys.TryGetValue(id, out GameObject obj))
                        {
                            NetworkServer.Destroy(obj);
                            GameObject.Destroy(obj);
                            Dummys.Remove(id);
                        }
                        break;
                    }

                default:
                    return new string[] { GetUsage() };
            }
            if (dummy == null) return new string[] { "Dummy failed to spawn" };
            Dummys.Add(dummyId, dummy);
            if (args.Length > 2 && float.TryParse(args[args.Length - 1], out float time))
            {
                MEC.Timing.CallDelayed(time, () =>
                {
                    NetworkServer.Destroy(dummy);
                    GameObject.Destroy(dummy);
                    Dummys.Remove(dummyId);
                });
                return new string[] { $"Dummy ({dummyId}) spawnned and will be removed in {time} seconds" };
            }
            else return new string[] { $"Dummy ({dummyId}) spawnned" };
        }
    }
}
