using System.Linq;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Gamer.Utilities;
using CommandSystem;
using Exiled.API.Features;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class PetCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "pets";

        public override string Description => "PETS";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "pet";

        public string GetUsage()
        {
            return "Pet [role] [name]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length < 2) return new string[] { GetUsage() };
            if(args[0] == "config-size")
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
            }

            var role = (RoleType)int.Parse(args[0]);
            var name = string.Join(" ", args.Skip(1));
            var player = sender.GetPlayer();

            if (Systems.Pets.PetsHandler.Pets.ContainsKey(player.UserId))
                Systems.Pets.PetsHandler.Pets.Remove(player.UserId);
            if(role != RoleType.None && role != RoleType.Spectator && role != RoleType.Scp079) 
                Systems.Pets.PetsHandler.Pets.Add(player.UserId, (role, name));
            Systems.Pets.PetsHandler.RefreshPets(player);
            _s = true;
            return new string[] { "Done" };
        }
    }
}
