
using CommandSystem;
using Gamer.Utilities;
using System.Linq;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class KillCommand : IBetterCommand, IPermissionLocked
    {

        public string Permission => "kill";

        public override string Description =>
        "Kill";
        public string PluginName => PluginHandler.PluginName;

        public override string Command => "kill";

        public override string[] Aliases => new string[] { "slay" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            string reason = "";
            if (args.Length > 1)
                reason = string.Join(" ", args.Skip(1)).Trim();
            var output = this.ForeachPlayer(args[0], out bool success, (player) =>
            {
                player.Kill(new DamageTypes.DamageType("Can not define what killed him"));
                player.Broadcast(5, $"<color=red>You have been killed by admin " + (reason != "" ? $"with reason {reason}" : "") + "</color>");
                return new string[] { "Done" };
            });
            if (!success) return new string[] { "Player not found", GetUsage() };
            _s = true;
            return output;
        }


        public string GetUsage()
        {
            return "Kill [Id] [Reason]";
        }
    }
}
