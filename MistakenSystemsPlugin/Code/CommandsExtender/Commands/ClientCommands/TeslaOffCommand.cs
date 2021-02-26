using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))] 
    class TeslaOffCommand : IBetterCommand
    {       
        public override string Description => "Disabled all tesla gates";
        public override string Command => "teslaOff";
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (player.Role != RoleType.NtfCommander)
                return new string[] { "Nie jesteś dowódcą" };
            if (Systems.Utilities.API.Map.TeslaMode == Systems.Utilities.API.TeslaMode.DISABLED)
                return new string[] { "Tesle są już wyłączone" };
            Systems.Utilities.API.Map.TeslaMode = Systems.Utilities.API.TeslaMode.DISABLED;
            Cassie.Message("Tesla gates deactivated by order of NINETAILEDFOX COMMANDER");
            success = true;
            return new string[] { "Zrobione" };
        }
    }
}
