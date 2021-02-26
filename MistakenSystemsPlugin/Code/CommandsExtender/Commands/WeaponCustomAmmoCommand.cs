
using Mirror;



using System.Linq;
using UnityEngine;

using System.Collections.Generic;
using Gamer.Utilities;
using CommandSystem;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class WeaponCustomAmmoCommand : IBetterCommand, IPermissionLocked
    {

        public string Permission => "wca";

        public override string Description =>
        "Weapon Custom Ammo";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "wca";

        public override string[] Aliases => new string[] { };

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            return new string[] { "WIP" };
            /*
            List<string> list = new List<string>();
            for (int i = 0; i < System.Enum.GetValues(typeof(WeaponCustomAmmo)).Length; i++)
            {
                list.Add(((WeaponCustomAmmo)i).ToString());
            }
            if (args.Length < 2) return new string[] { GetUsage(), string.Join("\n", list) };
            for (int i = 0; i < System.Enum.GetValues(typeof(WeaponCustomAmmo)).Length; i++)
            {
                if (((WeaponCustomAmmo)i).ToString().ToLower() == args[0].ToLower())
                {
                    if (!bool.TryParse(args[1], out bool value)) return new string[] { GetUsage() };
                    if (value) Systems.Utilities.API.Map.WCA.Add((WeaponCustomAmmo)i);
                    else Systems.Utilities.API.Map.WCA.Remove((WeaponCustomAmmo)i);
                    return new string[] { "Done" };
                }
            }
            return new string[] { "Unknown type", GetUsage() };*/
        }

        public string GetUsage()
        {
            return "WCA [Type] True/False\n ";
        }

    }
}
