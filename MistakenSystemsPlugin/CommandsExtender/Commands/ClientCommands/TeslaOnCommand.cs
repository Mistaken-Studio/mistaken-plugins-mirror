using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class TeslaOnCommand : IBetterCommand
    {
        public override string Description => "Enables all tesla gates";
        public override string Command => "teslaOn";

        internal static readonly HashSet<string> AlreadyUsed = new HashSet<string>();
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (player.Role != RoleType.NtfCommander)
                return new string[] { "Nie jesteś dowódcą" };
            if (Systems.Utilities.API.Map.TeslaMode == Systems.Utilities.API.TeslaMode.ENABLED)
                return new string[] { "Tesle są już włączone" };
            if (AlreadyUsed.Contains(player.UserId))
                return new string[] { "Możesz użyć .taslaOff lub .teslaOn tylko raz na runde" };
            Systems.Utilities.API.Map.TeslaMode = Systems.Utilities.API.TeslaMode.ENABLED;
            AlreadyUsed.Add(player.UserId);
            Cassie.Message("Tesla gates activated by order of NINETAILEDFOX COMMANDER");
            CassieRoom.CassieRoomHandler.TeslaIndicator.NetworkTargetState = false;
            success = true;
            return new string[] { "Zrobione" };
        }
    }
}
