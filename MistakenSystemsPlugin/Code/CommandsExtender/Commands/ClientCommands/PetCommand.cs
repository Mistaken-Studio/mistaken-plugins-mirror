using System.Linq;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Gamer.Utilities;
using CommandSystem;
using Exiled.API.Features;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))] 
    class PetCommand : IBetterCommand
    {
        public override string Command => "pet";

        public override string Description => "PETS";

        public string GetUsage()
        {
            return "Pet [name]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            var Sender = sender as CommandSender;
            if (!Sender.CheckPermission($"${PluginHandler.PluginName}.pets"))
            {
                if (!Ranks.RanksHandler.VipList.TryGetValue(Sender.SenderId, out Ranks.RanksHandler.PlayerInfo info) || (info.VipLevel != Ranks.RanksHandler.VipLevel.APOLLYON && info.VipLevel != Ranks.RanksHandler.VipLevel.KETER))
                    return new string[] { "Ta komenda jest tylko dla osób z Apollyonem lub Keterem" };
            }

            var player = sender.GetPlayer();
            bool enable = !Systems.Pets.PetsHandler.Pets.ContainsKey(player.UserId);
            if (args.Length < /*2*/1 && enable) 
                return new string[] { GetUsage() };
            /*if(args[0] == "config-size")
            {
                Systems.Pets.PetsHandler.PetSize = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                return new string[] { "Done" };
            }
            else if (args[0] == "config-speed")
            {
                Systems.Pets.PetsHandler.Speed = float.Parse(args[1]);
                return new string[] { "Done" };
            }
            else if (args[0] == "config-run")
            {
                Systems.Pets.PetsHandler.ShoudRun = bool.Parse(args[1]);
                return new string[] { "Done" };
            }*/

            var role = RoleType.None;//(RoleType)int.Parse(args[0]);
            var name = string.Join(" ", args/*.Skip(1)*/);
            if (string.IsNullOrWhiteSpace(name))
                name = $"Smol {player.DisplayNickname}";
            if (!enable)
                Systems.Pets.PetsHandler.Pets.Remove(player.UserId);
            else //if(role != RoleType.None && role != RoleType.Spectator && role != RoleType.Scp079) 
                Systems.Pets.PetsHandler.Pets.Add(player.UserId, (role, name));
            Systems.Pets.PetsHandler.RefreshPets(player);
            _s = true;
            return new string[] { enable ? "Enabled pet" : "Disabled pet" };
        }
    }
}
