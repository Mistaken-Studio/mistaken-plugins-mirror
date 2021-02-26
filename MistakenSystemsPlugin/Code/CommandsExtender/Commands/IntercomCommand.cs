

using System.Linq;
using UnityEngine;
using Assets._Scripts.Dissonance;
using Gamer.Utilities;
using CommandSystem;
using Exiled.API.Features;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class IntercomCommand : IBetterCommand, IPermissionLocked
    {
        
        public string Permission => "intercom";

        public override string Description =>
        "Intercom";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "intercom";

        public override string[] Aliases => new string[] { "int" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length < 1) return new string[] { GetUsage() };
            var output = this.ForeachPlayer(args[0], out bool success, (Player player) =>
            {
                if (player.Team == Team.SCP || player.Team == Team.RIP) return new string[] { "You have to be player" };
                DissonanceUserSetup dus = player.ReferenceHub.GetComponent<DissonanceUserSetup>();
                dus.TargetUpdateForTeam(player.Team);
                dus.SpectatorChat = false;
                dus.SCPChat = false;
                dus.RadioAsHuman = false;
                dus.MimicAs939 = false;
                dus.IntercomAsHuman = false;
                dus.ResetToDefault();

                if (bool.TryParse(args[1], out bool result))
                {
                    dus.IntercomAsHuman = result;
                    return new string[] { "Done: " + result };
                }

                return new string[] { "Error ?" };
            });
            _s = true;
            return output;
        }

        public string GetUsage()
        {
            return "Intercom (Id) true/false";
        }
    }
}