using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class LastVictimCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "last_attacker";

        public override string Description => "Get Last Victim";
        public string PluginName => PluginHandler.PluginName;

        public override string Command => "lastvictim";

        public override string[] Aliases => new string[] { "lv" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            var output = this.ForeachPlayer(args[0], out bool success, (player) =>
            {
                CommandsHandler.LastVictims.TryGetValue(player.UserId, out (Player, Player) info);
                string[] tor = new string[3];
                tor[0] = $"Data for {player.UserId}";
                tor[1] = $"Attacked: {info.Item1?.ToString(true) ?? "Not found"}";
                tor[2] = $"Killed: {info.Item2?.ToString(true) ?? "Not found"}";
                return tor;
            });
            if (!success)
                return new string[] { "Player not found", GetUsage() };
            _s = true;
            return output;
        }

        public string GetUsage()
        {
            return "LV (Id)";
        }
    }
}
