using CommandSystem;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    class ModRACommand : IBetterCommand
    {
        public override string Description => "Toggle default RA";

        public override string Command => "modRA";

        public string GetUsage() => "modRA true/false";

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0)
                return new string[] { GetUsage() };
            if (!bool.TryParse(args[0], out bool value))
                return new string[] { GetUsage() };
            if (!value) //false -> Default RA | true -> Modified RA
                LOFH.LOFHPatch.DisabledFor.Add(sender.GetPlayer().UserId);
            else
                LOFH.LOFHPatch.DisabledFor.Remove(sender.GetPlayer().UserId);
            _s = true;
            return new string[] { "Done" };
        }
    }
}
