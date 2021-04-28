using Gamer.Utilities;

namespace Gamer.Mistaken.Ranks
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.GameConsoleCommandHandler))]
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class UpdateTagCommand : IBetterCommand
    {
        public override string Command => "updatetag";

        public override string[] Aliases => new string[] { "utag" };

        public override string Description => "Updates Tags";

        public override string[] Execute(CommandSystem.ICommandSender sender, string[] args, out bool success)
        {
            RanksHandler.UpdateRoles();
            success = true;
            return new string[] { "Done" };
        }
    }

    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class RefreshTagCommand : IBetterCommand
    {
        public override string Command => "refreshtag";

        public override string[] Aliases => new string[] { "rtag" };

        public override string Description => "Refreshes Tags";

        public override string[] Execute(CommandSystem.ICommandSender sender, string[] args, out bool success)
        {
            RanksHandler.RoleType type = RanksHandler.RoleType.UNKNOWN;
            if (args.Length > 0 && int.TryParse(args[0], out int value))
                type = (RanksHandler.RoleType)value;

            var player = sender.GetPlayer();
            RanksHandler.ApplyRoles(player, type);

            success = true;
            return new string[] { "Done" };
        }
    }

    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class StaffTagCommand : IBetterCommand
    {
        public override string Command => "stafftag";

        public override string[] Aliases => new string[] { "stag" };

        public override string Description => "Staff Tags";

        public override string[] Execute(CommandSystem.ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            RanksHandler.ApplyStaffRoles(player);
            success = true;
            return new string[] { "Done" };
        }
    }
}
