using CommandSystem;
using Gamer.Utilities;
using Mirror;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class ClearMapCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "clearmap";

        public override string Description =>
        "CLEAR MAP";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "clearmap";

        public override string[] Aliases => new string[] { "cmap" };

        public string GetUsage()
        {
            return "CLEARMAP [RAGDOLL = FALSE]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            bool ragdoll = false;
            if (args.Length > 0)
                bool.TryParse(args[0], out ragdoll);
            foreach (var item in Pickup.Instances.ToArray())
                item.Delete();
            if (ragdoll)
            {
                foreach (var item in GameObject.FindObjectsOfType<Ragdoll>().ToArray())
                    NetworkServer.Destroy(item.gameObject);
            }
            _s = true;
            return new string[] { "Done" };
        }
    }
}
